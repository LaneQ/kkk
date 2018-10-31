var tok = typeof token !== "undefined" ? token : $('input[name="__RequestVerificationToken"]').val();
function spDetailsViewModel(data) {
    var self = this;
    self.Name = ko.observable("").extend({
        required: {
            params: true,
            message: "Please enter Name"
        },
        pattern: {
            params: "^[A-Za-z0-9][A-Za-z0-9\\s\/\\,\\.\\-\\_\\~\\`\\@\\#\\$\\&\\*\\;\\:\]{0,50}$",
            message: "Please enter maximum 50 characters"
        }
    });
    self.PhoneNumber = ko.observable("").extend({
        required: {
            params: true,
            message: "Please enter a phone number"
        },
        Number: {
            params: true,
            message: "This field must be numerical"
        },
        pattern: {
            params: "^\\d{8,10}$",
            message: "Please enter a valid phone number"
        }
    });
    self.Website = ko.observable("").extend(Extender.website());
    self.PhysicalAddress = new AddressViewModel();
    self.BillingAddress = new AddressViewModel();
    //function to move next
    function moveNext(element) {
        if (animating) return false;
        animating = true;

        current_fs = element.parent();
        next_fs = element.parent().next();

        //activate next step on progressbar using the index of next_fs
        $("#progressbar li").eq($("fieldset").index(next_fs)).addClass("active");

        //show the next fieldset
        next_fs.show();
        //hide the current fieldset with style
        current_fs.animate({ opacity: 0 }, {
            step: function (now, mx) {
                //as the opacity of current_fs reduces to 0 - stored in "now"
                //1. scale current_fs down to 80%
                scale = 1 - (1 - now) * 0.2;
                //2. bring next_fs from the right(50%)
                left = (now * 50) + "%";
                //3. increase opacity of next_fs to 1 as it moves in
                opacity = 1 - now;
                current_fs.css({ 'transform': 'scale(' + scale + ')' });
                next_fs.css({ 'left': left, 'opacity': opacity });
            },
            duration: 800,
            complete: function () {
                current_fs.hide();
                animating = false;
            },
            //this comes from the custom easing plugin
            easing: 'easeInOutBack'
        });
    }

    self.validationModel = ko.validation.group([
         self.Name,
         self.PhoneNumber,
    ]);
    self.detailValid = ko.computed(function () {
        return self.validationModel().length == 0;
    });
    //bug # 1025 Service supplier was unable to save at the end of onboarding
    self.modelErrors = ko.validation.group([
        self.PhysicalAddress.Number,
        self.PhysicalAddress.Street,
        self.PhysicalAddress.City,
        self.PhysicalAddress.PostCode,
        self.BillingAddress.Number,
        self.BillingAddress.Street,
        self.BillingAddress.City,
        self.BillingAddress.PostCode,
    ]);
    var interact = false;
    self.isValid = ko.computed(function () {
        var valid = self.modelErrors().length == 0;
        return valid;
    });
    self.ValidateServiceSupplier = function (data) {        
        if (self.detailValid()) {
            moveNext($('#nextServiceSupplier'));
        }
        else {
            self.validationModel.showAllMessages();
        }
    }
    self.onBoardingPage = ko.observable(true);
    self.ValidateSaveServiceSupplier = function (data) {
        if (self.modelErrors().length == 0) {
            self.savespDetails(data);
        }
        else {
            self.modelErrors.showAllMessages();
        }
    }
    self.ValidateSaveBeforeApply = function (data) {
        self.validationModel.showAllMessages();
        if (self.validationModel() == false) {
            self.saveBeforeApply(data);
        }
        else {
            return;
        }
    }

    self.removePhoto = function () {
        self.fileIstruction("Max size is 2Mb and supported file types are *.jpg, *.jpeg, *.png, *.gif.");
        self.fileWarning('');
        self.fileName('');
        self.fileSrc('');
        self.ImageFile('');
        //clearFileInput(document.getElementById("file-upload"));
    };
    self.toMain = function () {
        $('#regiestForm').css('display', 'none');
        $('#mainPage').css('display', 'block');
    }
    self.toNext = function () {
        self.validationModel.showAllMessages();
        if (self.validationModel() == false) {
            $('#firstPage').css('display', 'none');
            $('#secondPage').css('display', 'block');
            $('#cd').removeClass("active");
            $("#ca").addClass("active");
        }
        else {
        }
    }
    self.toPrev = function () {
        $('#firstPage').css('display', 'block');
        $('#secondPage').css('display', 'none');
        $('#ca').removeClass("active");
        $("#cd").addClass("active");
    }
    self.fileWarning = ko.observable("");
    self.fileIstruction = ko.observable("You may upload 1 file. Max size is 2 MB each and supported file types are *.jpg, *.jpeg & *.png.");
    self.ImageFile = ko.observable();
    self.fileName = ko.observable();
    self.fileSrc = ko.observable();
    self.IsShipSame = ko.observable(false);
    function clearFileInput(ctrl) {
        try {
            ctrl.value = null;
        } catch (ex) { }
        if (ctrl.value) {
            ctrl.parentNode.replaceChild(ctrl.cloneNode(true), ctrl);
        }
    }
    self.fileSelect = function (elemet, event) {
        var files = event.target.files;
        console.log(files);
        if (files && files[0]) {
            var f = files[0];
            if (!f.type.match('image.*')) {
                return;
            }
            var reader = new FileReader();
            reader.onload = function (e) {
                self.fileName(f.name);
                self.fileSrc(e.target.result);
                self.ImageFile(f);
            }
            reader.readAsDataURL(f);
        }
    };
    self.checkShip = ko.computed(function () {
        if (self.IsShipSame()) {
            interact = true;
            UpdateAddress(self.PhysicalAddress, self.BillingAddress);
        }
        else {
            if (interact) {
                clearAddress(self.BillingAddress);
            }
                
        }
    });
    self.saveBeforeApply = function (data, event) {
        //  console.log(data);
        var formData = new FormData();
        formData.append('Name', self.Name());
        formData.append('PhoneNumber', self.PhoneNumber());
        formData.append('Website', self.Website());
        formData.append('PhysicalAddress.Number', self.PhysicalAddress.Number());
        formData.append('PhysicalAddress.Street', self.PhysicalAddress.Street());
        formData.append('PhysicalAddress.Suburb', self.PhysicalAddress.Suburb());
        formData.append('PhysicalAddress.City', self.PhysicalAddress.City());
        formData.append('PhysicalAddress.PostCode', self.PhysicalAddress.PostCode());
        formData.append('PhysicalAddress.Latitude', self.PhysicalAddress.Latitude());
        formData.append('PhysicalAddress.Longitude', self.PhysicalAddress.Longitude());

        formData.append('BillingAddress.Number', self.BillingAddress.Number());
        formData.append('BillingAddress.Street', self.BillingAddress.Street());
        formData.append('BillingAddress.Suburb', self.BillingAddress.Suburb());
        formData.append('BillingAddresss.City', self.BillingAddress.City());
        formData.append('BillingAddress.PostCode', self.BillingAddress.PostCode());
        formData.append('BillingAddress.Latitude', self.BillingAddress.Latitude());
        formData.append('BillingAddress.Longitude', self.BillingAddress.Longitude());
        formData.append("imageFile", self.ImageFile());
        formData.append("IsShipSame", self.IsShipSame());
        formData.append('__RequestVerificationToken', tok);
        //var spDetails = ko.toJSON(data);
        $.ajax({
            type: 'POST',
            url: '/Companies/Onboarding/AddNewServiceProvider',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                console.log(" company added is :" + result.Success);
                if (result.Success) {
                    JobModel.updateProfile();
                    JobModel.toApplyDetail();
                }
            }
        });
    }
    self.savespDetails = function (data, event) {
        //  console.log(data);
        var form = $('#msform');
        var formData = new FormData();
        formData.append('Name', self.Name());
        formData.append('PhoneNumber', self.PhoneNumber());
        formData.append('Website', self.Website());
        formData.append('PhysicalAddress.Number', self.PhysicalAddress.Number());
        formData.append('PhysicalAddress.Street', self.PhysicalAddress.Street());
        formData.append('PhysicalAddress.Suburb', self.PhysicalAddress.Suburb());
        formData.append('PhysicalAddress.City', self.PhysicalAddress.City());
        formData.append('PhysicalAddress.PostCode', self.PhysicalAddress.PostCode());
        formData.append('PhysicalAddress.Latitude', self.PhysicalAddress.Latitude());
        formData.append('PhysicalAddress.Longitude', self.PhysicalAddress.Longitude());

        formData.append('BillingAddress.Number', self.BillingAddress.Number());
        formData.append('BillingAddress.Street', self.BillingAddress.Street());
        formData.append('BillingAddress.Suburb', self.BillingAddress.Suburb());
        formData.append('BillingAddress.City', self.BillingAddress.City());
        formData.append('BillingAddress.PostCode', self.BillingAddress.PostCode());
        formData.append('BillingAddress.Latitude', self.BillingAddress.Latitude());
        formData.append('BillingAddress.Longitude', self.BillingAddress.Longitude());
        formData.append("imageFile", self.ImageFile());
        formData.append("IsShipSame", self.IsShipSame());
        formData.append('__RequestVerificationToken', tok);
        //var spDetails = ko.toJSON(data);
        $.ajax({
            type: 'POST',
            url: '/Companies/Onboarding/AddNewServiceProvider',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                console.log(" company added is :" + result.Success);
                if (result.Success) {
                    window.location.replace('/Home/Dashboard'); //Bug Fix #2031
                }
                else {
                    window.location.replace('/Companies/Onboarding/Index');
                }
            }
        });
    }

    function clearAddress(address) {
        address.Number(null);
        address.Street(null);
        address.Suburb(null);
        address.City(null);
        address.PostCode(null);
        address.Latitude(null);
        address.Longitude(null);
    }
    function UpdateAddress(data, address) {
        data.Number ? address.Number(data.Number()) : 1;
        data.Street ? address.Street(data.Street()) : 1;
        data.Suburb ? address.Suburb(data.Suburb()) : 1;
        data.City ? address.City(data.City()) : 1;
        data.PostCode ? address.PostCode(data.PostCode()) : 1;
        data.Latitude ? address.Latitude(data.Latitude()) : 1;
        data.Longitude ? address.Longitude(data.Longitude()) : 1;
    }
}
ko.applyBindings(new spDetailsViewModel(), document.getElementById("CompanyDetailForm"));