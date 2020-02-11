using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
	public class ScheduleCustomerModel
	{
		[DisplayName("Customer Name")]
		[Required(ErrorMessage = "Customer name is required.")]
		[StringLength(50, MinimumLength = 1)]
		public string Name { get; set; }

		[DisplayName("Phone Number")]
		[DataType(DataType.PhoneNumber)]
		[Required(ErrorMessage = "Phone number is required.")]
		public string PhoneNumber { get; set; }

		[DisplayName("Barber Name")]
		[Required(ErrorMessage = "Barber name is required.")]
		[StringLength(50, MinimumLength = 1)]
		public string BarberName { get; set; }
	}
}