using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Customer
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TcNo { get; set; } = string.Empty; // Turkish Cypriot ID Number
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }


}