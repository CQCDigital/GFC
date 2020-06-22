using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SYE.ViewModels
{
    public class SearchVm
    {
        [DisplayName("Search by service name or address")]
        [MaxLength(1000, ErrorMessage = "Your search must be 1,000 characters or less")]
        [Required(ErrorMessage = "Enter the name of a service, its address, postcode or a combination of these")]
        public string SearchTerm { get; set; }
    }
}
