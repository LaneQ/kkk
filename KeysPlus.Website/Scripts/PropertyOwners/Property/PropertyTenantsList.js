
var e = window.location.search.substring(1).split("&");

var newpropertyId = e[1].split('=')[1];
var templateID = e[2].split('=')[1];
console.log(newpropertyId);
console.log(templateID);

function PropertyTenants(item) {

    var self = this;
    self.Id = ko.observable(item.Id);
    self.DropdownId = 'tenantDetail_' + item.Id;
    self.DropdownHref = '#tenantDetail_' + item.Id;
    self.DropdownId2 = 'tenantMessagge_' + item.Id;
    self.DropdownHref2 = '#tenantMessagge_' + item.Id;
    self.TenantName = ko.observable(item.TenantName);
    self.TenantId = ko.observable(item.TenantId);
    self.TenantEmail = ko.observable(item.TenantEmail).extend(Extender.email);
    self.TenantPhone = ko.observable(item.TenantPhone);
    self.RentAmount = ko.observable(item.RentAmount);
    self.RentFrequency = ko.observable(item.RentFrequency);
    self.StartDate = ko.observable(item.StartDate).extend({
        date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        dateAfterYearBuilt: self.Year,
    });
    self.EndDate = ko.observable(item.EndDate).extend({
        date: true,
        dateAfterStart: self.StartDate
    });


    self.PropertyId = ko.observable(item.PropertyId);
    self.PropertyAddress = ko.observable(item.PropertyAddress);
    self.ProfilePicture = ko.observable(item.ProfilePicture);
    self.ProfilePicture = item.ProfilePicture || new FileModel({ Data: '/images/profile_avatar.png' });
    self.MediaFiles = ko.observableArray();
    //  self.FirstFoto = self.ProfilePicture()  || new FileModel({ Data: '/images/profile_avatar.png' });
    self.FirstFoto = self.MediaFiles()[0] || new FileModel({ Data: '/images/profile_avatar.png' });
    self.PaymentAmount = ko.observable().extend(Extender.decimalNumeric());
}


function PropertyTenantModel(data) {

    var self = this;
    self.newpropId = ko.observable();
    self.PropertyTenantList = ko.observableArray();
    self.SelectedPropertyTenants = ko.observable();
    self.SelectedTenant = ko.observable();

    for (var key in data) {
        self[key] = data[key]
    }

    self.YearBuilt = ko.observable("").extend({
        required: {
            params: true,
            message: "Please enter the Year Built."
        },

        pattern: {
            params: "^(19\\d\\d|200[0-9]|201[0-7]){0,4}$",
            message: "The Year Built field must be a number and is from 1900 to present."
        }
    });
    if (templateID == 2) { // templateID 2 calls the Add Tenant template
        self.showTenantList = ko.observable(false);
        self.showAddTenant = ko.observable(true);
    }
    else {
        self.showTenantList = ko.observable(true);
        self.showAddTenant = ko.observable(false);
    }
    self.TenantToPropertyModel = new PropertyTenants(self.YearBuilt());
    self.TenantErrors = ko.validation.group(self.TenantToPropertyModel, { deep: true, live: true });
    self.IsTenantValid = ko.computed(function () {
        return self.TenantErrors().length == 0;
    });

    self.removeTenantModal = function (data) {
        self.SelectedTenant(data);
        console.log(self.SelectedTenant());
        $('#mainPage').css('display', 'none');
        $('#removeTenantModal').css('display', 'block');
    };

    self.toMain = function () {
        $('#mainPage').css('display', 'block');
        $('#removeTenantModal').css('display', 'none');
    };

    self.removeTenant = function (data) {
        console.log(data);
        var forSaving = ko.toJSON(data);

        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/RemoveTenantFromProperty',
            data: forSaving,
            contentType: 'application/json',
            success: function (result) {
                self.PropertyTenantList.remove(self.SelectedTenant());
                self.toMain();
            },
            error: function () { },
            fail: function () { }
        });
        console.log(self.PropertyTenantList());

    };


    self.AddTenantToProperty = function (data) {
        console.log(data);
        debugger;
        console.log("Submiting tenant", data);
        var forSaving = ko.toJSON(data.TenantToPropertyModel);
        console.log(forSaving);


        var tenantData = ko.toJS(data.TenantToPropertyModel);
        tenantData = {
            TenantEmail: data.TenantToPropertyModel.TenantEmail(),
            StartDate: data.TenantToPropertyModel.StartDate(),
            EndDate: data.TenantToPropertyModel.EndDate(),
            PaymentFrequencyId: data.TenantToPropertyModel.PaymentFrequencyId,
            PaymentAmount: data.TenantToPropertyModel.PaymentAmount,
            PropertyId: newpropertyId,
        };

        console.log(tenantData);
        debugger;
        $.ajax({
            type: "POST",
            url: '/PropertyOwners/Property/AddTenantToProperty',
            data: tenantData,
            async: false,

        }).done(function (response) {
            console.log(response);
            if (response.Todo && response.Todo == 'Send email') {
                var result = confirm("Tenant does not exist in the system.Do you wish your tenant to be registered to the community?");
                if (result) {
                    var tenantData = ko.toJS(self.TenantToPropertyModel);
                    tenantData = {
                        TenantEmail: self.TenantToPropertyModel.TenantEmail(),
                        StartDate: self.TenantToPropertyModel.StartDate(),
                        EndDate: self.TenantToPropertyModel.EndDate(),
                        PaymentFrequencyId: self.TenantToPropertyModel.PaymentFrequencyId,
                        PaymentAmount: self.TenantToPropertyModel.PaymentAmount,
                        PropertyId: newpropertyId
                    };

                    $.ajax({
                        type: "POST",
                        url: '/PropertyOwners/Onboarding/SendInvitationEmailToTenant',
                        data: tenantData,
                        async: false,
                        success: function (data) {
                            debugger;
                            if (data.Success) {
                                window.location.replace("/PropertyOwners/Home/Index");
                            }
                        },
                    });
                }

            }
            window.location.replace("/PropertyOwners/Home/Index");
        });

    };

    self.AddTenantForm = function () {

        console.log("add tenant form");
        self.showAddTenant(true);
        self.showTenantList(false);
        self.newpropId = self.PropertyId;

    };

    self.BackToTenantList = function () {

        self.showAddTenant(false);
        self.showTenantList(true);
        self.newpropId = self.PropertyId;

    };

};





