﻿using System;
using AutoMapper;

using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using asp_net_po_schedule_management_server.Dto;
using asp_net_po_schedule_management_server.DbConfig;
using asp_net_po_schedule_management_server.Entities;
using asp_net_po_schedule_management_server.Exceptions;
using asp_net_po_schedule_management_server.Services.Helpers;


namespace asp_net_po_schedule_management_server.Services.ServicesImplementation
{
    public sealed class StudyGroupServiceImplementation : IStudyGroupService
    {
        private readonly IMapper _mapper;
        private readonly ServiceHelper _helper;
        private readonly ApplicationDbContext _context;
        
        //--------------------------------------------------------------------------------------------------------------
        
        public StudyGroupServiceImplementation(IMapper mapper, ApplicationDbContext context, ServiceHelper helper)
        {
            _mapper = mapper;
            _context = context;
            _helper = helper;
        }

        //--------------------------------------------------------------------------------------------------------------

        #region Add new study group

        /// <summary>
        /// Metoda odpowiadająca za stworzenie nowej grupy dziekańskiej i dodanie jej do bazy danych. Metoda sprawdza
        /// przed stworzeniem, czy nie zachodzi próba dodania duplikatu, jeśli tak rzuci wyjątek.
        /// </summary>
        /// <param name="dto">obiekt transferowy z danymi</param>
        /// <returns>informacje o stworzonej grupie dziekańskiej</returns>
        /// <exception cref="BasicServerException">brak zasobu/próba wprowadzenia duplikatu</exception>
        public async Task<List<CreateStudyGroupResponseDto>> CreateStudyGroup(CreateStudyGroupRequestDto dto)
        {
            // wyszukanie kierunku oraz wydziału, jeśli nie znajdzie wyrzuć wyjątek
            StudySpecialization findStudySpec = await _context.StudySpecializations
                .Include(s => s.Department)
                .Include(s => s.StudyType)
                .Include(s => s.StudyDegree)
                .FirstOrDefaultAsync(s => 
                    string.Equals(s.Name + " (" + s.StudyType.Alias + " " + s.StudyDegree.Alias + ")", 
                        dto.StudySpecName, StringComparison.OrdinalIgnoreCase) &&
                    s.Department.Name.Equals(dto.DepartmentName, StringComparison.OrdinalIgnoreCase));
            if (findStudySpec == null) {
                throw new BasicServerException("Nie znaleziono pasującego wydziału/kierunku", HttpStatusCode.NotFound);
            }

            // wyszukanie wszystkich pasujących semestrów, jeśli nic nie znajdzie wyrzuć wyjątek
            List<Semester> findAllSemesters = await _context.Semesters
                .Where(s => dto.Semesters.Any(id => id == s.Id))
                .ToListAsync();
            if (findAllSemesters.Count == 0) {
                throw new BasicServerException("Nie znaleziono pasujących semestrów.", HttpStatusCode.NotFound);
            }

            List<StudyGroup> createdAllStudyGrops = new List<StudyGroup>();
            
            for (int i = 0; i < dto.CountOfGroups; i++) { // stwórz tyle grup ile przesłano w zapytaniu
                foreach (Semester semester in findAllSemesters) { // i przypisz do tylu semestrów ile w zapytaniu
                    StudyGroup createdStudyGroup = new StudyGroup()
                    {
                        Name = $"{findStudySpec.StudyDegree.Alias} {findStudySpec.Alias} " +
                               $"{findStudySpec.StudyType.Alias} {semester.Alias.Substring(4)}/{i + 1}",
                        DepartmentId = findStudySpec.Department.Id,
                        StudySpecializationId = findStudySpec.Id,
                        SemesterId = semester.Id,
                    };
                    // znajdowanie duplikatów, jeśli znajdzie rzuć wyjątek
                    StudyGroup findExistingStudyGroup = await _context.StudyGroups
                        .FirstOrDefaultAsync(g => g.Name.Equals(createdStudyGroup.Name));
                    if (findExistingStudyGroup != null) {
                        throw new BasicServerException("Podana grupa istnieje już w systemie.",
                            HttpStatusCode.ExpectationFailed);
                    }
                    createdAllStudyGrops.Add(createdStudyGroup);
                }
            }
            
            await _context.StudyGroups.AddRangeAsync(createdAllStudyGrops);
            await _context.SaveChangesAsync();
            
            // spłaszczanie i zwracanie wyniku/wyników
            return createdAllStudyGrops.Select(s => _mapper.Map<CreateStudyGroupResponseDto>(s)).ToList();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region Get all study groups

        /// <summary>
        /// Metoda zwracająca wszystkie grupy dziekańskie opakowane w obiekt paginacji i fitrowania rezultatów na podstawie
        /// przekazywanych parametrów zapytania. Umożliwia sortowanie po kolumnach (kluczach) w trybach ASC/DES.
        /// </summary>
        /// <param name="searchQuery">parametry zapytania (filtrowania wyników)</param>
        /// <returns>opakowane dane wynikowe w obiekt paginacji</returns>
        public PaginationResponseDto<StudyGroupQueryResponseDto> GetAllStudyGroups(SearchQueryRequestDto searchQuery)
        {
            // wyszukiwanie użytkowników przy pomocy parametru SearchPhrase
            IQueryable<StudyGroup> studyGroupsBaseQuery = _context.StudyGroups
                .Include(r => r.Department)
                .Include(r => r.Semester)
                .Include(r => r.StudySpecialization)
                .Where(r => searchQuery.SearchPhrase == null ||
                            r.Name.Contains(searchQuery.SearchPhrase, StringComparison.OrdinalIgnoreCase));

            // sortowanie (rosnąco/malejąco) dla kolumn
            if (!string.IsNullOrEmpty(searchQuery.SortBy)) {
                _helper.PaginationSorting(new Dictionary<string, Expression<Func<StudyGroup, object>>>
                {
                    { nameof(StudyRoom.Id), r => r.Id },
                    { nameof(StudyRoom.Name), r => r.Name },
                    { "DepartmentAlias", r => r.Department.Alias },
                    { "StudySpecAlias", r => r.StudySpecialization.Alias },
                }, searchQuery, ref studyGroupsBaseQuery);
            }
            
            List<StudyGroupQueryResponseDto> allGroups = _mapper.Map<List<StudyGroupQueryResponseDto>>(_helper
                .PaginationAndAdditionalFiltering(studyGroupsBaseQuery, searchQuery));
            
            return new PaginationResponseDto<StudyGroupQueryResponseDto>(
                allGroups, studyGroupsBaseQuery.Count(), searchQuery.PageSize, searchQuery.PageNumber);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region Delete content

        /// <summary>
        /// Metoda usuwająca wybrane grupy dziekańskie z bazy danych (na podstawie wartości id w ciele zapytania).
        /// </summary>
        /// <param name="studyGroups">wszystkie numery ID elementów do usunięcia</param>
        /// <param name="credentials">obiekt autoryzacji na podstawie claimów</param>
        public async Task DeleteMassiveStudyGroups(MassiveDeleteRequestDto studyGroups, UserCredentialsHeaderDto credentials)
        {
            await _helper.CheckIfUserCredentialsAreValid(credentials);
            // filtrowanie sal zajęciowych po ID znajdujących się w tablicy
            _context.StudyGroups.RemoveRange(_context.StudyGroups
                .Where(r => studyGroups.ElementsIds.Any(id => id == r.Id)));
            await _context.SaveChangesAsync();
        }

        //--------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Metoda usuwająca z bazy danych wszystkie grupy dziekańskie.
        /// </summary>
        /// <param name="credentials">obiekt autoryzacji na podstawie claimów</param>
        public async Task DeleteAllStudyGroups(UserCredentialsHeaderDto credentials)
        {
            await _helper.CheckIfUserCredentialsAreValid(credentials);
            _context.StudyGroups.RemoveRange();
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}