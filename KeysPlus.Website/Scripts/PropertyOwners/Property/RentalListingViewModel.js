function RentalListingViewModel() {
    var self = this;
    self.pid = ko.observable(PropId);
    self.validateNow = ko.observable(false);
    self.currentView = ko.observable('RentalListing');
    self.Title = ko.observable().extend(Extender.title);
    self.PropertyId = ko.observable().extend({
        min: {
            params: 1,
            message: "Please select a property",             
            onlyIf: function () {
                return self.validateNow();
            }
        }
        
    });
    self.RentalDescription = ko.observable().extend(Extender.descriptionRentalListing);
    self.MovingCost = ko.observable().extend({
        required: {
            message: "Please enter a moving cost."
        },
        Number: {
            params: true,
        },
        pattern: {
            params: /^[0-9]{1,5}(\.[0-9]{1,2})?$/,
            message: "Please enter a valid numeric amount upto 2 decimal places"
        }
    });
    self.TargetRent = ko.observable().extend({
        required: {
            params: true,
            message: "Please enter a target rent."
        },
        Number: {
            params: true,
        },
        pattern: {
            params: /^[0-9]{1,8}(\.[0-9]{1,2})?$/,
            message: "Please enter a valid numeric amount upto 2 decimal places"
        }
    });
    self.AvailableDate = ko.observable().extend({
        date: true,
        required: {
            params: true,
            message: "Please enter a date."
        }
    });

    self.Furnishing = ko.observable().extend(Extender.furnishing);
    self.IdealTenant = ko.observable().extend(Extender.idealTenant);
    self.OccupantCount = ko.observable().extend({
        required: {
            params: true,
            message: "Please enter the count of occupants."
        },
        Number: {
            params: true,
        },
        pattern: {
            params: /^[0-9]{0,5}$/,
            message: "Please enter a valid numeric amount."
        }
    });
    self.PetsAllowed = ko.observable().extend({ required: { param: true, message: "Please Select a Choice." } });
    self.MediaFiles = ko.observableArray();
    self.FileWarning = ko.observable(KeysInstrucTion.fileUpLoad);
    self.AvailableProperties = ko.observableArray();
    self.ValidFileTypes = ["image/gif", "image/jpeg", "image/png", "image/jpg"];
    self.RemovePhoto = function (photo) {
        self.MediaFiles.remove(photo);
    };
    
    self.errorInput = ko.validation.group([self.PropertyId,self.Title, self.RentalDescription, self.TargetRent, self.AvailableDate, self.MovingCost, self.OccupantCount]);

    self.validInput = ko.computed(function () {       
        console.log(self.errorInput().length);        
        return self.errorInput().length == 0 && self.PropertyId() != 0;
    });
    function getAvailableProperties() {
        console.log("Getting Available Properties", self.pid());
        if (self.pid() == null) {
            $.ajax({
                type: 'GET',
                url: '/Property/GetAllProperties',

                success: function (result) {
                    self.AvailableProperties.removeAll();
                    var propertyList = JSON.parse(result);
                    debugger;
                    propertyList.forEach(function (property) {
                        self.AvailableProperties.push(property);
                    });
                    console.log(self.AvailableProperties());
                },
                error: function () { },
                fail: function () { },
            });
        }
        else {
            $.ajax({
                type: 'GET',
                url: '../Property/GetSpecificProperties/',
                data: { pid: self.pid() },
                success: function (result) {
                    var propertyList = JSON.parse(result);
                    self.AvailableProperties.removeAll();
                    propertyList.forEach(function (property) {
                        self.AvailableProperties.push(property);
                    });
                    console.log(self.AvailableProperties());
                },
                error: function () { debugger },
                fail: function () { debugger },
            });
        }
    }
    self.SubmitRentalListing = function (data) {
        var formData = new FormData();
        var title = data.Title();
        formData.append('Title', title.trim());
        if (data.PropertyId() != 0) {
            formData.append('PropertyId', data.PropertyId());
        }
        else if (data.PropertyId() == 0){
            self.validateNow(true);
            if (!self.validInput()) {
                self.error.showAllMessages();
            }
        }
        else {
            
            formData.append('PropertyId', self.pid());
        }
        
        formData.append('RentalDescription', data.RentalDescription());
        formData.append('MovingCost', data.MovingCost());
        formData.append('TargetRent', data.TargetRent());
        formData.append('AvailableDate', KeysUtils.toDotnetDate(data.AvailableDate()) );
        formData.append('Furnishing', data.Furnishing());
        formData.append('IdealTenant', data.IdealTenant());
        formData.append('OccupantCount', data.OccupantCount());
        formData.append('PetsAllowed', data.PetsAllowed());
        var addedPhotos = self.MediaFiles();
        for (var i = 0; i < addedPhotos.length; i++) {
            formData.append("MediaFiles" + i, addedPhotos[i].File);
        }

        if (confirm("Do you want to list it to Rental?")) {
            $.ajax({
                type: 'POST',
                url: '../Property/AddRentalListing',
                data: formData,
                contentType: false,
                processData: false,
                success: function (result) {
                    console.log("Success");
                    KeysUtils.notification.show('<p>Property listed as rental successfully!</p>', 'success', reload);
                    
                },
                error: function (err) {
                    KeysUtils.notification.show(KeysUtils.notification.errorMsg, 'error');
                    return;
                },
                fail: function () { debugger },
            });
        }
        else {
            self.Title("");
            self.PropertyId("");
            self.RentalDescription("");
            self.MovingCost("");
            self.TargetRent("");
            self.Furnishing("");
            self.IdealTenant("");
            self.OccupantCount("");
            self.AvailableDate("");
            self.PetsAllowed("");
            $('#rentalListingModal').modal("hide");
        }

        function reload() {
            window.location.replace("/PropertyOwners/Property/RentalProperties");
        };
    }
    //this.errorInput.showAllMessages();
}
function OptionProperty(property) {
    var self = this;
    self.Id = property.Id;
    self.Name = property.Name;
    if (property.Address) {
        self.AddressString = KeysUtils.fullAddress(property.Address);
    }

}
var PetObj = function (choice) {
    this.Choice = choice;
};

var PetsAllowedOption = ko.observableArray([
    new PetObj("Yes"),
    new PetObj("No")
]);
ko.applyBindings(new RentalListingViewModel());