namespace WebApplication3.Models
{
	public class ScheduledCustomerModel
	{
		public int AppointmentID { get; set; }
		public string Name { get; set; }
		public string PhoneNumber { get; set; }
		public string BarberNameRequested { get; set; }
		public string CurrentBarberAssigned { get; set; }
		public decimal WaitTime { get; set; }
	}
}