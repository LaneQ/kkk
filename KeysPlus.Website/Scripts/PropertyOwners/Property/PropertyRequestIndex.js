var token = $('input[name=__RequestVerificationToken]').val();
function PropertyTenantRequest(item) {
    var self = this;
    self.PropertyId = ko.observable(item.PropertyId);
    self.PropertyImages = ko.observableArray(item.PropertyImages);
    self.FirstPhoto = self.PropertyImages()[0] || new FileModel({ Data: '/images/no-prop-img.png' });
    self.PropertyAddress = ko.observable(item.PropertyAddress);
    self.Address = ko.observable(item.Address)
    self.StreetAddress = KeysUtils.toAddressStreet(item.Address);
    self.CitySub = KeysUtils.toCitySub(item.Address);
    //self.TenantName = ko.observable(item.TenantName);
    //self.TenantContactNumber = ko.observable(item.TenantContactNumber);
    self.NewRequestsCount = ko.observable(item.NewRequestsCount);
    //self.TenantJobRequests = ko.observableArray();
    //item.TenantJobRequests.forEach(function (JobRequest) {
    //    self.TenantJobRequests.push(new TenantJobRequest(JobRequest));
    //});
    //debugger;
}

function TenantJobRequest(item) {
    var self = this;
    
      selfTenantJobRequest = this;
    self.TenantJobRequestId = ko.observable(item.TenantJobRequestId);
    self.IsAccepted = ko.observable(item.IsAccepted);
    self.RequestType = ko.observable(item.RequestType);
    self.JobDescription = ko.observable().extend(Extender.description);
    self.JobDescription(item.JobDescription);
    self.IsViewed = ko.observable(item.IsViewed);
    self.RequestStatus = ko.observable(item.RequestStatus);
    self.DateCreated = ko.observable(item.DateCreated);
    self.IsUpdated = ko.observable(item.IsUpdated);
    self.IsAccepted = ko.observable(item.IsAccepted);
    self.MaxBudget = ko.observable().extend(Extender.decimalNumeric());
    self.PropertyId = ko.observable(item.PropertyId);
    self.MediaFiles = ko.observableArray(item.MediaFiles);
    self.PhotoFiles = ko.observableArray();
    self.Errors = ko.validation.group(self);
    self.SelectedMediaFileRadioButton = ko.observable(false);
    self.IsValid = ko.computed(function () {
        return self.Errors().length ==0;
    });



    function MediaViewModel(file) {
        debugger;
        this.Id = file.Id;
        this.File = file.File;
        this.Data = file.Data;
        this.Status = file.Status;
        this.active = file.active;
    }

    var mapOptions = {
        "ignore": ["CreatedBy", "UpdatedBy", "UpdatedOn"],
        "MediaFiles": {
            create: function (options) {
                console.log(options);
                if (options != null) {
                    return new MediaViewModel(options.data);
                }
            }
        }
    };
}

function AddJobRequest(item) {
    var self = this;
       self.Id = -1;
    self.TenantJobRequestId=ko.observable(item.TenantJobRequestId);
    self.PropertyId = ko.observable(item.PropertyId);
    self.JobDescription = ko.observable(item.JobDescription);
    self.MaxBudget = ko.observable(item.MaxBudget);
    self.MediaFiles = ko.observable(item.MediaFiles)
}

var PropertyTenantJobRequestViewModel = function (data) {
    var self = this;
    self.firstVisibility = ko.observable(true);
    self.setVisibility = ko.observable(false);
    self.showMarketPlace = ko.observable(false);
    self.SelectedRadioButton = ko.observable("true");
    self.PropertyTenantRequests = ko.observableArray();
    self.SelectedPropertyTenantRequest = ko.observable();
    self.SelectedTenantJobRequest = ko.observable();
    data.forEach(function (item) {
        self.PropertyTenantRequests.push(new PropertyTenantRequest(item));
    });

    // computed function for create a job for marcket place
    self.showInputForm = ko.computed({
        read: function () {
            if (self.SelectedRadioButton() == "true") { return true; }
            else { return false; }
        },
        write: function () { }
    });


    self.validFileTypes = [
        "application/pdf",
        "application/msword",
        "image/gif",
        "image/jpeg",
        "image/png"];


    self.validImageTypes = [        
        "image/gif",
        "image/jpeg",
        "image/png"];

    self.RemoveFile = function (file) {
        self.SelectedTenantJobRequest().MediaFiles.remove(file);
    };

    self.fileUpload = function (data, event) {

        console.log(self.SelectedTenantJobRequest().MediaFiles());
        var files = event.target.files;
        console.log(files);
        var file = files[0];
        var maxFileSize = 5000000;
        for (var i = 0; i < files.length; i++) {
            //debugger;
            if (typeof (maxFileSize) != "undefined" && file.size >= maxFileSize) {
                $("#ImageFile").val("");
                $("#SizeNotSupported").modal("show");
                break;
            }
            if (!~self.validFileTypes.indexOf(file.type)) {

                break;
            }
            else {
                (function (file) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        var result = e.target.result;

                        file = {
                            File: file,
                            Data: result,
                            OldFileName: file.name,
                            Status: "load"
                        }
                        //  selfTenantJobRequest.MediaFiles
                        self.SelectedTenantJobRequest().MediaFiles.push(file);
                        //debugger;
                        console.log(file);

                    };
                    reader.readAsDataURL(file);
                })(files[i]);
            }
        }
    }
    // go to main index page and reset all the data
    self.goToIndex = function () {
        self.firstVisibility(true);
        self.setVisibility(false);
        self.showMarketPlace(false);
        self.showInputForm(false);
        //self.showMediaForm(false);
    };
    
    self.pageNumber = ko.observable(0);
    self.nbPerPage = 10;
    self.totalPages = ko.computed(function () {
        if (self.SelectedPropertyTenantRequest() != null) {       
            var div = Math.floor(self.SelectedPropertyTenantRequest().TenantJobRequests().length / self.nbPerPage);
            div += self.SelectedPropertyTenantRequest().TenantJobRequests().length % self.nbPerPage > 0 ? 1 : 0;
            console.log("Total PAge",div);
            return div -1;
        }
    });

    self.paginated = ko.computed(function () {
        if (self.SelectedPropertyTenantRequest() != null) {
            var first = self.pageNumber() * self.nbPerPage;
            console.log("First", first);
            return self.SelectedPropertyTenantRequest().TenantJobRequests().slice(first, first + self.nbPerPage);
        }
    });
    self.hasPrevious = ko.computed(function () {
        return self.pageNumber() !== 0;
    });
    self.hasNext = ko.computed(function () {
        return self.pageNumber() !== self.totalPages();
    });
    self.next = function () {
        if (self.pageNumber() < self.totalPages()) {
            self.pageNumber(self.pageNumber() + 1);
        }
    }

    this.previous = function () {
        if (self.pageNumber() != 0) {
            self.pageNumber(self.pageNumber() - 1);
        }
    }
    
    // listing all requests for particular property
    self.viewTenantRequests = function (data) {
        //debugger;   
       
        self.SelectedPropertyTenantRequest(data);
        console.log(self.SelectedPropertyTenantRequest().TenantJobRequests().length, self.totalPages(), self.paginated());
        self.firstVisibility(false);
        self.setVisibility(true);
    }

    // view details pop-up for particular tenant request and changing IsViewed to true
    self.showTenantRequestDetailsModal = function (data) {
        console.log(data);
        console.log(data.MediaFiles());
        console.log(data.MediaFiles().length);
        var requestFiles = data.MediaFiles();
        

        for (var i = 0; i < requestFiles.length; i++) {
            if (self.validImageTypes.indexOf(requestFiles[i].type) < 0) {

                console.log("Not an image");
                console.log(requestFiles[i]);
                console.log(requestFiles[i].File);
            }
            else {

                console.log("An image");
                console.log(requestFiles[i]);
                console.log(requestFiles[i].File);
              //  self.PhotoFiles.push(photo);
            }
        }

        self.showMarketPlace(false);
        //data.JobDescription();
        data.MaxBudget("");
        self.SelectedTenantJobRequest(data);
        $('#tenantRequestDetails').appendTo("body").modal('show');
        $('#tenantRequestDetails').appendTo("body").on("shown.bs.modal", function () {
            $(this).find("[autofocus]:first").focus();
            $.ajax({
                type: 'POST',
                url: '/Property/RequestViewed',
                data: { id :data.TenantJobRequestId},
                success: function (result) {
                    //$("#RequestCount").load(location.href + " #RequestCount");
                    //$("#newRequestCount").load('@Url.Action("PropertyRequestsCount", "Property", new { Area = "PropertyOwners" })');
                    
                    if (result.Updated) {
                        self.SelectedTenantJobRequest().IsViewed(true);
                        var count = self.SelectedPropertyTenantRequest().NewRequestsCount()-1;
                        self.SelectedPropertyTenantRequest().NewRequestsCount(count);
                    }
                },
                error: function () { },
                fail: function () { }
            });
        });
    }

    // closing the "tenantRequestDetails" pop-up and reseting the values
    self.closeTenantRequestDetailsModal = function () {
        $('#tenantRequestDetails').modal('hide');
        self.showMarketPlace(false);
        self.showInputForm(false);
      //  self.showMediaForm(false);
    }

    // changing the content in the pop-up modal
    self.showConfirmationForPostInMarketPlace = function (re) {
        if (re() == "Job Request") {
            self.isMarketPlace(true);
        }
        else {
            self.isMarketPlace(false);
        }
        self.showMarketPlace(true);
        $('#mybutton').prop('disabled', true);
        self.SelectedRadioButton("false");
    }
   
    self.isMarketPlace = ko.observable("false");
    // Add JobRequest to Market Place (TenantJobRequest Table)
    self.AddJobToMarketPlace = function (data) {
        console.log(data);
        console.log(data.MediaFiles());
        //var newjob = new AddJobRequest(self.SelectedTenantJobRequest());
        var newjob = self.SelectedTenantJobRequest();
        $('#tenantRequestDetails').modal('hide');
      console.log(newjob);


       // //var formData = new FormData($('#upload_form')[0]);
       // var formData = new FormData($('#upload_form').get(0));
        var formData = new FormData();
        formData.append('JobDescription', data.JobDescription());
        formData.append('PropertyId', data.PropertyId());
        formData.append('MaxBudget', data.MaxBudget());
        formData.append('TenantJobRequestId', data.TenantJobRequestId());
        var fileCopy = [];
        data.MediaFiles().forEach(function (element) {
            if (element.Id) {
                fileCopy.push(element.Id);
                formData.append('FilesCopy', element.Id);
 
            }
        });
       
 
   //     formData.append('MediaFiles', data.MediaFiles());
        //formData.append('FilesCopy', fileCopy);
        var addedPhotos = data.MediaFiles();
        console.log(self.SelectedTenantJobRequest().MediaFiles());
        console.log(addedPhotos);
 

        for (var i = 0; i < addedPhotos.length; i++) {
            
       //     console.log(addedPhotos)
            formData.append("MediaFiles1", addedPhotos[i].File);
 

            
        }
        //debugger;
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }



        $.ajax({


            //type: 'POST',
            //url: '/Property/AddJobRequest',
            //data: ko.toJSON(newjob),
            //contentType: 'application/json',
       
            type: 'POST',
            url: '/Property/AddJobRequest',
            data: formData,//ko.toJSON(newjob), //
            //dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Posted) {
                    $('#ConfirmationForPostInMarketPlace').appendTo('body').modal('show');
                    $('#ConfirmationForPostInMarketPlace').on('shown.bs.modal', function () {
                        $(this).find("[autofocus]:first").focus();
                    });
                }
            },
            error: function () { },
            fail: function () { }
        });

    }

    // Accepted job Request and do it by property owner
    self.AddJob = function (data) {
        var newjob = new AddJobRequest(self.SelectedTenantJobRequest());
        var postdata = ko.toJS(newjob);
        debugger;
        $('#tenantRequestDetails').modal('hide');    
        $.ajax({
            type: 'POST',
            url: '/Jobs/Home/SaveJob',
            data: { __RequestVerificationToken: token, jobViewModel : postdata},
            //contentType: 'application/json',
            success: function (result) {
                  
            },
            error: function () { },
            fail: function () { }
        });

    }

}