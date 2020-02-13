import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { BarberAndWaitTime } from '../../BarberAndWaitTime';
import { ScheduledCustomer } from '../../ScheduledCustomer';
import { CustomerToSchedule } from '../CustomerToSchedule';
import { ScheduleAppointmentFeedback } from '../../ScheduleAppointmentFeedback';


@Component({
  selector: 'app-barber-scheduler',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})


export class AppComponent implements OnInit {
  public constructor(private titleService: Title, public http: HttpClient) {
  }
  scheduleAppointmentFeedback: ScheduleAppointmentFeedback;
  customerToSchedule: CustomerToSchedule;

  scheduledCustomers: ScheduledCustomer[];
  barbersAndWaitTimes: BarberAndWaitTime[];

  ngOnInit() {
    this.customerToSchedule = new CustomerToSchedule();
    this.customerToSchedule.Name = '';
    this.customerToSchedule.PhoneNumber = '';
    this.customerToSchedule.BarberName = '';

    this.scheduleAppointmentFeedback = new ScheduleAppointmentFeedback();
    this.scheduleAppointmentFeedback.FeedbackDefault = true;
    this.scheduleAppointmentFeedback.FeedbackSuccess = false;
    this.scheduleAppointmentFeedback.FeedbackWarning = false;
    this.scheduleAppointmentFeedback.FeedbackDanger = false;
    this.scheduleAppointmentFeedback.FeedbackMessage = '';

    this.getAllScheduledCustomers();
  }

  getBarbersAndWaitTimes(): void {
    this.http.post<any>('../../Barber/GetEstimatedTime', JSON.stringify({}), { headers: { 'Content-Type': 'application/json;charset=UTF-8' } })
      .subscribe(
        data => { this.barbersAndWaitTimes = data; },
        error => { alert(error) }
      )
  }

  getAllScheduledCustomers(): void {
    this.http.post<any>('../../Barber/GetScheduledCustomers', JSON.stringify({}), { headers: { 'Content-Type': 'application/json;charset=UTF-8' } })
      .subscribe(
        data => {
          this.scheduledCustomers = data;
          this.getBarbersAndWaitTimes();
        },
        error => { alert(error) }
      )
  }

  scheduleCustomer(): void {
    this.http.post<any>('../../Barber/ScheduleCustomer', JSON.stringify({
      Name: this.customerToSchedule.Name,
      PhoneNumber: this.customerToSchedule.PhoneNumber,
      BarberName: this.customerToSchedule.BarberName
    }), { headers: { 'Content-Type': 'application/json;charset=UTF-8' }} )
      .subscribe(
        data => {
          // clear out the data by default.
          this.scheduleAppointmentFeedback.FeedbackDefault = true;
          this.scheduleAppointmentFeedback.FeedbackSuccess = false;
          this.scheduleAppointmentFeedback.FeedbackWarning = false;
          this.scheduleAppointmentFeedback.FeedbackDanger = false;
          this.scheduleAppointmentFeedback.FeedbackMessage = '';

          // set the success message in our alert box.
          this.scheduleAppointmentFeedback.FeedbackMessage = data;

          // determine the styling.
          var alertType: string = '';

          if (data.startsWith('Success:')) {
            alertType = 'Success';
          } else if (data.startsWith('Warning:')) {
            alertType = 'Warning';
          } else if (data.startsWith('Error:')) {
            alertType = 'Error';
          }

          switch (alertType) {
            case 'Success':
              this.scheduleAppointmentFeedback.FeedbackSuccess = true;
                  break;

            case 'Warning':
              this.scheduleAppointmentFeedback.FeedbackWarning = true;
                  break;

            case 'Error':
              this.scheduleAppointmentFeedback.FeedbackDanger = true;
                  break;

            default:
              this.scheduleAppointmentFeedback.FeedbackDefault = true;
                  break;
          }

          // after we schedule a customer, we need to retrieve the list again.
          this.getAllScheduledCustomers()
        },
        error => {
          // set the error message in our alert box.
          alert(error)
        }
      )
  }

  moveScheduledCustomer(appointmentID: number, barberNameRequested: string, barberScheduleType: number): void {
    this.http.post<any>('../../Barber/SetCustomerInBarberChair',
      JSON.stringify({
        AppointmentID: appointmentID,
        BarberNameRequested: barberNameRequested,
        BarberScheduleType: barberScheduleType
      }), { headers: { 'Content-Type': 'application/json;charset=UTF-8' } })
      .subscribe(
        data => {
          this.getAllScheduledCustomers();
        },
        error => {
          alert(error)
        }
      )

  }
}
