
function PropertyOwnerNewJob(item) {

    var self = this;
    self.PropertyId = ko.observable();
    self.JobDescription = ko.observable().extend(Extender.description);
    self.MaxBudget = ko.observable().extend(Extender.decimalNumeric());
    self.Title = ko.observable().extend(Extender.title);;
    self.MediaFiles = ko.observableArray();
    self.DocFiles = ko.observableArray();
    self.NewJobErrors = ko.validation.group(this, { deep: true, live: true });
    self.IsJobValid = ko.computed(function () {
        return self.NewJobErrors().length == 0;
    });


    function photoViewModel(photo) {
        this.Id = photo.Id;
        this.File = photo.File;
        this.Data = photo.Data;
        this.Status = photo.Status;
        this.active = photo.active;
    }
    self.validImageTypes = [
        "image/gif",
        "image/jpeg",

    ];
    self.validFileTypes = [
        "image/jpeg",
        "image/png",
        "image/gif",
        "application/pdf",
        "application/msword",
        "docx"
    ];
    //ignore couple of fields
    var mapOptions = {
        "ignore": ["CreatedBy", "UpdatedBy", "UpdatedOn"],
        "MediaFiles": {
            create: function (options) {
                console.log(options);
                if (options != null) {
                    return new photoViewModel(options.data);
                }
            }
        }
    };
    self.viewModel = ko.mapping.fromJS(item, mapOptions);
    self.viewModel.copy = item;
    self.viewModel.mapOptions = mapOptions;
    self.validationModel = ko.validatedObservable(self.viewModel);

    self.viewModel.disableItem = ko.observable(true);

    //upload documents for the New JOb
    self.fileUpload = function (data, event) {
        var maxFileSize = 500000;
        var files = event.target.files;
        var photoAllowedAmount = 5;
        var validationMessage = '';
        //check photo amount
        if (files.length + self.MediaFiles().length > photoAllowedAmount) {
            validationMessage = "Please upload no more than " + photoAllowedAmount + " photos";
            console.log("too many photos");
        } else {
            //self.validationModel.isValid(true);
            for (var i = 0; i < files.length; i++) {
                //validate size of each photo
                if (files[i].size >= maxFileSize) {
                    validationMessage = "File size is too large. Maximum allowed size is upto 5MB.";
                    console.log("too big File");
                    break;
                }
                //validate type of each photo, invalid photo will return -1
                if (self.validFileTypes.indexOf(files[i].type||files[i].name.split('.').pop()) < 0) {
                    validationMessage = "Supported file types are *.jpg, *.jpeg, *.png, *.doc, *.docx,*.pdf, *.gif";
                    console.log("unsupported photos");
                    break;
                }
                else {
                    //if every photo is at the right size and right photo type
                    (function (file) {
                        //file is a single photo uploaded
                        var reader = new FileReader();
                        reader.onload = function (e) {
                            var result = e.target.result;
                            var photo = {
                                File: file,
                                Data: result,
                                OldFileName: file.name,
                                Name: file.name,
                                Status: "add",
                                MediaType: KeysFiles.getType(file),
                            };
                            self.MediaFiles.push(photo);
                        };
                        reader.readAsDataURL(file);
                    })(files[i]);  // files[i] is each photo passed to the function
                }
            }
        }

        //change validation message corresponding to interaction
        $('#photo_validation').text(validationMessage).css('color', 'red');
        //$("#uploadBtn").val('');// 
    };
    //remove docs from the list
    self.removePhoto = function (photo) {
        console.log("remove photo");
        self.MediaFiles.remove(photo);
    };
    // Add Job to the Market
    self.SubmitMarketJob = function (data, event) {
        event.preventDefault();
        var postData = ko.toJSON(data);
        console.log(postData);
        var formData = new FormData();
        formData.append('MaxBudget', data.MaxBudget());
        formData.append('JobDescription', data.JobDescription());
        formData.append('PropertyId', data.PropertyId());
        formData.append('Title', data.Title());
        var addedPhotos = self.MediaFiles();
        for (var i = 0; i < addedPhotos.length; i++) {
            formData.append("MediaFiles" + i, addedPhotos[i].File);
        }

        for (var pair of formData.entries()) {
            console.log(pair[0] + ',' + pair[1] + ',' + pair[2] + ',' + pair[3]);
        }

        $.ajax({
            type: 'POST',
            url: '/Jobs/Home/AddJobToMarket',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                console.log(result);
                //KeysUtils.notification.show('<p>New job added successfully!</p>', 'success', reload);
                if (result.Success) {
                    KeysUtils.notification.show('<p>Job added successfully!</p>', 'success', reload);
                    //window.location.replace("/Jobs/Home/GetMarketJobs?isOwner=True");
                }
            }
        });
    }
}
function reload() {
    window.setTimeout(function () {
        KeysUtils.goPage("/Home/Dashboard");
    },1000);
}