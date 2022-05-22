﻿using System.Threading.Tasks;
using System.Collections.Generic;

using asp_net_po_schedule_management_server.Dto;


namespace asp_net_po_schedule_management_server.Services
{
    public interface IStudyRoomsService
    {
        Task<CreateStudyRoomResponseDto> CreateStudyRoom(CreateStudyRoomRequestDto dto);
        PaginationResponseDto<StudyRoomQueryResponseDto> GetAllStudyRooms(SearchQueryRequestDto searchQuery);
        Task<List<NameWithDbIdElement>> GetAllStudyRoomsScheduleBaseCath(long deptId, long cathId);
        Task DeleteMassiveStudyRooms(MassiveDeleteRequestDto studyRooms, UserCredentialsHeaderDto credentials);
        Task DeleteAllStudyRooms(UserCredentialsHeaderDto credentials);
    }
}