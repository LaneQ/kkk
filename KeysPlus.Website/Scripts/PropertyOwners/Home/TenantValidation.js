$(function () {
   
    var tenantArea = $("#tenant-area");
    ko.validation.init({
        insertMessages: true,
        decorateElement: true,
        errorElementClass: 'has-error',
        errorMessageClass: 'help-block'
    });
    $(document).ready(function () {
        $('input#decimal').blur(function () {
            var num = parseFloat($(this).val());
            var cleanNum = num.toFixed(2);
            $(this).val(cleanNum);
        });
    });
    function tenantViewModel() {
        var self = this;
        self.TenantEmail = ko.observable().extend({
            required: {
                params: true,
                message: "Please enter email"
            },
            pattern: {
                params: "^[a-z0-9][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)?@[a-z][a-zA-Z-0-9]*\.[a-z]+(\.[a-z]+)?$",
                message: "Please include valid Email."
            }
        });
        self.StartDate = ko.observable("").extend({
            date: true, required: { params: true, message: "Please enter start date." }
        });

        self.EndDate = ko.observable("").extend({
            date: true, min: this.StartDate, message: "In valid date!"
        });
        self.PaymentFrequencyId = ko.observable('1');
        self.PaymentAmount = ko.observable().extend({
            required: {
                params: true,
                message: "Please Enter PaymentAmount."
            },
            Number: {
                params: true,
            },
            maxLength: 9,
            pattern: {
                params: "^[0-9]+(.[0-9]{1,9})?$",
                message: "Please enter an amount upto 9 digits only."
            }
        });

        //var startDateString = $('#startDate').val();
        //var startMomentObj = moment(startDateString, 'MM-DD-YYYY');
        //startMomentObj = moment(startMomentObj).format('DD/MM/YYYY');
        //var endDateString = $('#endTDate').val();
        //var endMomentObj = moment(endDateString, 'MM-DD-YYYY');
        //endMomentObj = moment(endMomentObj).format('DD/MM/YYYY');

        //self.addTenant = function (data, event) {
        //    event.preventDefault();
        //    var form = $('#addTenantForm');
        //    if (form.valid()) {
        //        var startDateString = $('#startDate').val();
        //        var startMomentObj = moment(startDateString, 'MM-DD-YYYY');
        //        startMomentObj = moment(startMomentObj).format('DD/MM/YYYY');
        //        var endDateString = $('#endTDate').val();
        //        var endMomentObj = moment(endDateString, 'MM-DD-YYYY');
        //        endMomentObj = moment(endMomentObj).format('DD/MM/YYYY');
        //        var values = form.serializeArray();
        //        for (index = 0; index < values.length; ++index) {
        //            if (values[index].name == "StartDate") {
        //                values[index].value = startMomentObj;
        //                break;
        //            }
        //            if (values[index].name == "EndDate") {
        //                values[index].value = endMomentObj;
        //                break;
        //            }
        //        }
        //        values = jQuery.param(values);
        //        debugger
        //        $.ajax({
        //            type: "POST",
        //            url: '/PropertyOwners/Manage/AddTenantToProperty',
        //            data: values,
        //            success: function (response) {
        //                if (response.Todo && response.Todo == 'Send email') {
        //                    var result = confirm("Tenant does not exist in the system.Do you wish your tenant to be registered to the community?");
        //                    if (result) {
        //                        $.ajax({
        //                            type: "POST",
        //                            url: '/PropertyOwners/Manage/SendInvitationEmailToTenant',
        //                            data: values,
        //                            success: function (data) {
        //                            }
        //                        });
        //                    }
        //                }
        //            }
        //        });
        //    }
        //}
        var copy;
        var succeessss = false;
        self.addTenant = function (data) {
            //  console.log(" start date : ", startMomentObj, "endate : ", endMomentObj);
            console.log(data);
            console.log(prop.PropId());
            var Tenant = {};
            Tenant.TenantEmail = self.TenantEmail;
            Tenant.StartDate = self.StartDate;
            Tenant.EndDate = self.EndDate;
            Tenant.PaymentFrequencyId = self.PaymentFrequencyId;
            Tenant.PaymentAmount = self.PaymentAmount;
            Tenant.PropertyId = prop.PropId();
            var forSaving = ko.toJSON(Tenant);
            copy = forSaving;
            console.log(forSaving);
            $.ajax({
                type: "POST",
                url: '/PropertyOwners/Manage/AddTenantToProperty',
                data: forSaving,
                //method: "POST",
                contentType: 'application/json',
                success: function (response) {
                    //  if (response.redirect == 'Redirect')
                    // window.location = response.url;
                    //window.location ='/PropertyOwners/Home/Index';
                    if (response.Todo && response.Todo == 'Send email') {
                        succeessss = true;
                        var result = confirm("Tenant does not exist in the system.Do you wish your tenant to be registered to the community?");
                        if (result) {
                            console.log(forSaving);
                           
                            $.ajax({
                                type: "POST",
                                url: '/PropertyOwners/Manage/SendInvitationEmailToTenant',
                                data: { "TenantEmail": self.TenantEmail, "StartDate": self.StartDate, "EndDate": self.EndDate, "PaymentFrequencyId": self.PaymentFrequencyId, "PaymentAmount": self.PaymentAmount, "PropertyId": prop.PropId() },
                                success: function (data) {
                                    if (data.Success) {
                                        window.location.replace("Home/Index");
                                    }
                                },
                            });
                        }
                        
                    }
                    window.location = '/PropertyOwners/Home/Index';
                }
            });
           

        }
    };
    tenantViewModel.errors = ko.validation.group(tenantViewModel);
    ko.applyBindings(new tenantViewModel(), document.getElementById('tenant-area'));
});