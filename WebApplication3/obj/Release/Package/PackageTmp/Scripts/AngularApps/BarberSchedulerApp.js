(function (angular) {
    'use strict';
    angular.module('barberScheduleApp', [])
        .controller('scheduledCustomersController', function ($http) {
            
            var sccc = this;

            // Default Data, empty list of customers.
            sccc.scheduledCustomers = [];

            // List of barbers and how long their wait time is.
            sccc.barbersAndWaitTimes = [];

            sccc.getBarbersAndWaitTimes = function () {
                $http.post(
                    '../../Barber/GetEstimatedTime',
                    JSON.stringify({}) // pass in nothing.
                ).then(function successCallback(response) {
                    sccc.barbersAndWaitTimes = response.data;
                }, function errorCallback(response) {
                    alert(response);
                });
             }

            sccc.getAllScheduledCustomers = function () {
                $http.post(
                    '../../Barber/GetScheduledCustomers',
                    JSON.stringify({}) // pass in nothing.
                ).then(function successCallback(response) {
                    sccc.scheduledCustomers = response.data;
                }, function errorCallback(response) {
                     alert(response);
                });
            }

            sccc.moveScheduledCustomer = function (appointmentID, barberNameRequested, barberScheduleType) {
                $http.post(
                    '../../Barber/SetCustomerInBarberChair',
                    JSON.stringify({
                        AppointmentID: appointmentID,
                        BarberNameRequested: barberNameRequested,
                        BarberScheduleType: barberScheduleType
                    }) // pass in nothing.
                ).then(function successCallback(response) {
                    sccc.getAllScheduledCustomers();
                    sccc.getBarbersAndWaitTimes();
                }, function errorCallback(response) {
                    alert(response);
                });
            }

            sccc.scheduleCustomer = function () {
                if ($('form').valid()) {
                    var name = $('#Name').val();
                    var phoneNumber = $('#PhoneNumber').val();
                    var barberName = $('#BarberName').val();

                    $http.post(
                        '../../Barber/ScheduleCustomer',
                        JSON.stringify({
                            Name: name,
                            PhoneNumber: phoneNumber,
                            BarberName: barberName
                        }) // pass in nothing.
                    ).then(function successCallback(response) {
                        var alertType = 'Success';

                        if (response.data.startsWith('Success:')) {
                            alertType = 'Success';
                        } else if (response.data.startsWith('Warning:')) {
                            alertType = 'Warning';
                        } else if (response.data.startsWith('Error:')){
                            alertType = 'Error';
                        }

                        openAlert(response.data, alertType)

                        sccc.getAllScheduledCustomers();
                        sccc.getBarbersAndWaitTimes();
                    }, function errorCallback(response) {
                        alert(response);
                    });

                }
            }
    })
})(window.angular);