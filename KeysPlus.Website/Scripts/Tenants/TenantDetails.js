var tok = typeof token !== "undefined" ? token : $('input[name="__RequestVerificationToken"]').val();

ko.validation.rules.pattern.message = 'Invalid.';

ko.validation.init({
    registerExtenders: true,
    messagesOnModified: true,
    insertMessages: true,
    parseInputAttributes: true,
    messageTemplate: null
}, true);

function TenantAddress() {
    var self = this;

    self.Number = ko.observable();
    self.Street = ko.observable();
    self.Suburb = ko.observable();
    self.City = ko.observable();
    self.Region = ko.observable();
    self.Latitude = ko.observable();
    self.Longitude = ko.observable();
    self.Country = ko.observable();

    self.confirmationModal = function () {
        $('.ui.small.modal').modal('show');
    }

    self.closeConfirmation = function () {
        $('.ui.small.modal').modal('hide');
    }


}

function TenantInfoVm() {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;

    self.MediaFiles = ko.observableArray();
    self.FileWarning = ko.observable("");
    self.ValidFiles = ["image/gif", "image/jpeg", "image/png", "image/jpg"];
    self.MaxFiles = 1;
    self.MaxSize = 2000000;
    self.fileInstructions = "You may upload 1 image. Maximum size is 2 MB and supported file types are *.jpg, *.jpeg *.png & *.gif";
    self.fileSizeError = "2MB";
    self.filter = document.getElementById("file-upload").accept = "image/*";
    self.TooOldMessage = ko.observable("You must be younger than 150 to become a tenant.");
    self.TooYoungMessage = ko.observable("You must be older than 18 to become a tenant.");

    self.RemoveFile = function (file) {
        self.MediaFiles.remove(file);
    }

    self.confirmationModal = function () {
        $('.ui.small.modal').modal('show');
    }

    self.closeConfirmation = function () {
        $('.ui.small.modal').modal('hide');
    }


    self.Model = new Entity(KeysExtendsDic.TenantInfo);
    self.Model.PhotoFile = ko.observable();
    self.Model.DateOfBirth = ko.observable().extend({ required: true });

    self.AddressField = ko.observable().extend({
        required: {
            params: true,
            message: "Please enter address."
        }
    });
    self.Model.Address = new TenantAddress();


    self.Errors = ko.validation.group([self.Model, self.Model.DateOfBirth, self.AddressField], { deep: true });



    self.IsValid = ko.computed(function () {

        return self.Errors().length == 0;
    });

    self.SaveInfo = function (data) {
        if (self.IsValid()) {
            // Stop save tenant info if tenant is too young or too old
            //if (self.Model.DateOfBirth.hasError() == false && self.Model.DateOfBirth.tooOld() == false) {
                var formData = KeysUtils.toData(data.Model);
                formData.append("__RequestVerificationToken", token);
                $.ajax({
                    type: 'POST',
                    url: '/Tenants/Home/Onboarding',
                    data: formData,
                    dataType: 'json',
                    contentType: false,
                    processData: false,
                    success: function (result) {
                        if (result.Success) {
                            KeysUtils.notification.show('<p>Details updated successfully!</p>', 'success');
                            window.location.replace('/Home/Dashboard');
                            return;
                        }
                        else {
                            KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                        }
                    }
                });
            //}
        }
        else {
            self.Errors.showAllMessages();
        }
    }
}