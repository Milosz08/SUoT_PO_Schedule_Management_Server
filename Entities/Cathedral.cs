﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using asp_net_po_schedule_management_server.Entities.Shared;


namespace asp_net_po_schedule_management_server.Entities
{
    [Table("cathedrals")]
    public class Cathedral : PrimaryKeyWithClientIdentifierInjection
    {
        [Required]
        [StringLength(100)]
        [Column("cath-name")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(20)]
        [Column("cath-alias")]
        public string Alias { get; set; }
        
        [Required]
        [Column("if-removable")]
        public bool IfRemovable { get; set; } = true;
        
        [ForeignKey(nameof(Department))]
        [Column("dept-key")]
        public long DepartmentId { get; set; }
        
        public virtual Department Department { get; set; }
    }
}