using System;
using System.Web.Mvc;
using System.Collections.Generic;
using WebApplication2.Models;
using System.Linq;
using WebApplication2.Utility;

namespace WebApplication2.Controllers
{
	public class BarberController : Controller
	{
		[HttpPost]
		public JsonResult ScheduleCustomer(ScheduleCustomerModel scheduleCustomerModel)
		{
			if (scheduleCustomerModel is null)
				return Json("Error: Object cannot be null.");

			if (!c_listOfBarbers.Contains(scheduleCustomerModel.BarberName))
				return Json("Error: Barber must be one of the following: " + string.Join(", ", c_listOfBarbers));

			if (string.IsNullOrWhiteSpace(scheduleCustomerModel.Name))
				return Json("Error: Must provide a customer name.");

			if (string.IsNullOrWhiteSpace(scheduleCustomerModel.PhoneNumber))
				return Json("Error: Must provide a phone number.");

			// get list of already scheduled customers from session.
			List<ScheduledCustomerModel> currentScheduledCustomers = scheduledCustomers;

			// Check to see we aren't scheduling the same customer twice.
			if (currentScheduledCustomers.Any(a => a.Name.ToLower() == scheduleCustomerModel.Name.ToLower() && a.PhoneNumber == scheduleCustomerModel.PhoneNumber))
				return Json("Warning: Customer is already scheduled.");

			ScheduledCustomerModel newScheduledCustomerModel = new ScheduledCustomerModel
			{
				AppointmentID = GetAppointmentId(),
				Name = scheduleCustomerModel.Name,
				PhoneNumber = scheduleCustomerModel.PhoneNumber,
				BarberNameRequested = scheduleCustomerModel.BarberName
			};

			bool barberWasAvailable = false;

			// If the barber requested is not busy, assign them to that barber.
			if (!currentScheduledCustomers.Any(a => a.CurrentBarberAssigned == scheduleCustomerModel.BarberName))
			{
				barberWasAvailable = true;
				newScheduledCustomerModel.CurrentBarberAssigned = scheduleCustomerModel.BarberName;
			}

			currentScheduledCustomers.Add(newScheduledCustomerModel);

			// update the session data.
			scheduledCustomers = currentScheduledCustomers;

			if (barberWasAvailable)
			{
				return Json("Success: Customer is seeing barber.");
			}
			else
			{
				int positionInLine = currentScheduledCustomers.Where(w => w.CurrentBarberAssigned is null).Count();
				string ordinalFormatting = OrdinalUtility.ToOrdinal(positionInLine);

				// exclude the customers who are already currently assigned a barber.
				return Json(String.Format("Success: Customer is {0}{1} in line.", positionInLine, ordinalFormatting));
			}
		}

		[HttpPost]
		public JsonResult GetScheduledCustomers()
		{
			return Json(scheduledCustomers);
		}

		[HttpPost]
		public JsonResult SetCustomerInBarberChair(
			int appointmentId, string barberNameRequested, BarberScheduleType barberScheduleType)
		{
			List<ScheduledCustomerModel> currentScheduledCustomers = scheduledCustomers;

			// Identify the customer sitting in the chair with this AppointmentID.
			ScheduledCustomerModel customerInRow = currentScheduledCustomers
				.SingleOrDefault(a => a.AppointmentID == appointmentId);

			// identify the barber that has the lowest wait time.
			List<Tuple<string, decimal>> barbersAndWaitTimes = GetBarberNamesAndWaitTimes();

			if (barberScheduleType == BarberScheduleType.InChair)
			{
				// Find the customer we need to remove assigned to this barber.
				ScheduledCustomerModel customerToRemove = currentScheduledCustomers
					.Where(w => w.CurrentBarberAssigned == barberNameRequested).FirstOrDefault();
				if (customerToRemove != null)
					currentScheduledCustomers.Remove(customerToRemove);

				// Update the AppointmentID passed in to be assigned the barber desired.
				foreach (ScheduledCustomerModel cust in currentScheduledCustomers)
				{
					if (cust.AppointmentID == customerInRow.AppointmentID)
						cust.CurrentBarberAssigned = customerInRow.BarberNameRequested;
				}
			}
			else if (barberScheduleType == BarberScheduleType.NextAvailableBarber)
			{
				// It's possible their preferred barber is the one with lowest wait time.
				// In this situation, lets favor their desired barber.
				decimal minWaitTime = barbersAndWaitTimes.Select(s => s.Item2).Min();

				// find barbers with the min wait time.
				List<string> barbersWithMinWaitTime = barbersAndWaitTimes
					.Where(w => w.Item2 == minWaitTime)
					.Select(s => s.Item1)
					.ToList();

				// if none of the barbers with lowest wait times are the preferred barber, use another barber.
				if (!barbersWithMinWaitTime.Any(a => a == barberNameRequested))
					barberNameRequested = barbersWithMinWaitTime.First();

				// Find the customer we need to remove assigned to this barber.
				ScheduledCustomerModel customerToRemove = currentScheduledCustomers
					.Where(w => w.CurrentBarberAssigned == barberNameRequested).FirstOrDefault();
				if (customerToRemove != null)
					currentScheduledCustomers.Remove(customerToRemove);

				// Update the Customer passed in to now be assigned to the barber they requested.
				foreach (ScheduledCustomerModel cust in currentScheduledCustomers)
				{
					if (cust.AppointmentID == appointmentId)
						cust.CurrentBarberAssigned = barberNameRequested;
				}
			}
			else if (barberScheduleType == BarberScheduleType.MarkDone)
			{
				string barberNameNowUnassigned = customerInRow.CurrentBarberAssigned;

				// Remove the customer in row, because we've marked them as done.
				if (customerInRow != null)
					currentScheduledCustomers.Remove(customerInRow);

				// find the Customer that is next in line to sit in this barber's chair.
				ScheduledCustomerModel nextCustomerToBeAssigned = currentScheduledCustomers
					.Where(w => w.BarberNameRequested == barberNameNowUnassigned)
					.OrderBy(o => o.AppointmentID) // Find the customer that has been waiting the longest.
					.FirstOrDefault();

				if (nextCustomerToBeAssigned != null)
				{
					foreach (ScheduledCustomerModel cust in currentScheduledCustomers)
					{
						if (cust.AppointmentID == nextCustomerToBeAssigned.AppointmentID)
						{
							// careful here, the barberNameRequested here is not what we want to assign.
							cust.CurrentBarberAssigned = nextCustomerToBeAssigned.BarberNameRequested;
						}
					}
				}
			}

			// update the Session data.
			scheduledCustomers = currentScheduledCustomers;

			return Json("Success");
		}

		[HttpPost]
		public JsonResult GetEstimatedTime()
		{
			List<Tuple<string, decimal>> barbersAndWaitTimes = GetBarberNamesAndWaitTimes();

			return Json(barbersAndWaitTimes.Select(s => new {
				BarberName = s.Item1,
				WaitTime = s.Item2,
				WaitTimeSuccess = s.Item2 == 0,
				WaitTimeWarning = s.Item2 > 0 && s.Item2 < 45,
				WaitTimeDanger= s.Item2 >= 45
			}));
		}

		private List<Tuple<string, decimal>> GetBarberNamesAndWaitTimes()
		{
			List<ScheduledCustomerModel> currentScheduledCustomers = scheduledCustomers;

			var barbersAndWaitTimes = c_listOfBarbers.Select(barber => new
			{
				BarberName = barber,
				WaitTimes = scheduledCustomers
					.Where(w =>
						// Find Customers that have requested this barber, AND are not assigned to a different barber.
						(w.BarberNameRequested == barber
						 && (w.CurrentBarberAssigned is null || w.CurrentBarberAssigned == barber) ) ||
						// Find Customers that did not request this barber, but were assigned them.
						(w.BarberNameRequested != barber && w.CurrentBarberAssigned == barber)
					)
					.Select(z => (decimal?)z.WaitTime)
					.OrderByDescending(o => o)
			})
				.Select(s =>
				{
					decimal waitTime = 0;

					decimal? maxWaitTime = s.WaitTimes.FirstOrDefault();

					if (maxWaitTime != null)
					{
						waitTime = maxWaitTime.Value + (c_cleanupTime + c_timeToCutHair);
					};

					return new { s.BarberName, waitTime };
				}
				).ToList();

			return barbersAndWaitTimes
				.Select(s => new Tuple<string, decimal>(s.BarberName, s.waitTime))
				.ToList();
		}

		private List<string> c_listOfBarbers = new List<string>{ "Joe", "Gary" };

		// Session data persists for 20 minutes by default, can extend up to 60 minutes.
		private List<ScheduledCustomerModel> scheduledCustomers
		{
			get {
				// by default this is null.
				List<ScheduledCustomerModel> customers =
					Session["ScheduledCustomers"] as List<ScheduledCustomerModel>;

				return customers != null
					? customers
					: new List<ScheduledCustomerModel>();
			}
			set {
				List<ScheduledCustomerModel> customers = value;

				// before storing the value, calculate the wait times.
				int i = 0;
				foreach (ScheduledCustomerModel customer in customers)
				{
					// Find customers without barbers.
					if (customer.CurrentBarberAssigned == null)
					{
						decimal countCustomersAhead = scheduledCustomers
							// all customers with lower AppointmentID that aren't the same customer.
							.Where(w => (w.AppointmentID < customer.AppointmentID
										 && w.AppointmentID != customer.AppointmentID)
									// possible that a customer at end of queue is bumped up into barber chair.
									|| w.CurrentBarberAssigned == customer.BarberNameRequested)
							.Where(w => w.CurrentBarberAssigned == customer.BarberNameRequested
									|| w.BarberNameRequested == customer.BarberNameRequested)
							.Count();

						customer.WaitTime = (decimal)(countCustomersAhead * (c_cleanupTime + c_timeToCutHair));
					}
					else
					{
						customer.WaitTime = 0;
					}

					i++;
				}

				Session["ScheduledCustomers"] = customers
					.OrderBy(o => o.WaitTime)
					.ThenBy(tb => tb.BarberNameRequested).ToList();
			}
		}

		private int GetAppointmentId()
		{
			// by default this is null
			int? appointmentIdSession = Session["AppointmentID"] as int?;

			int appointmentId = 0;

			// if we've already requested an AppointmentID, it will be not null, which means we have to increment.
			if (appointmentIdSession.HasValue)
				appointmentId = appointmentIdSession.Value + 1;

			// Set the AppointmentID value, there are two situations:
			// First: value will be null, we set it to 0, and don't increment.
			// Second and subsequent calls: value will be 0 or greater, so we increment.
			Session["AppointmentID"] = appointmentId;

			return appointmentId;
		}

		public enum BarberScheduleType
		{
			InChair = 1,
			NextAvailableBarber = 2,
			MarkDone = 3
		}

		private const decimal c_cleanupTime = 1.5m;
		private const decimal c_timeToCutHair = 15m;
	}
}