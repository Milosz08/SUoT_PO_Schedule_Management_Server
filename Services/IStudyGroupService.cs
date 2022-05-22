﻿using System.Threading.Tasks;
using System.Collections.Generic;

using asp_net_po_schedule_management_server.Dto;


namespace asp_net_po_schedule_management_server.Services
{
    public interface IStudyGroupService
    {
        Task<List<CreateStudyGroupResponseDto>> CreateStudyGroup(CreateStudyGroupRequestDto dto);
        PaginationResponseDto<StudyGroupQueryResponseDto> GetAllStudyGroups(SearchQueryRequestDto searchQuery);
        Task DeleteMassiveStudyGroups(MassiveDeleteRequestDto studyGroups, UserCredentialsHeaderDto credentials);
        Task DeleteAllStudyGroups(UserCredentialsHeaderDto credentials);
    }
}