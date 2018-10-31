
var quoteModel = function (item) {
    var self = this;
    self.Id = ko.observable(item.Id);
    self.PropertyAddress = ko.observable(item.PropertyAddress);
    self.JobDescription = ko.observable(item.JobDescription);
    self.Note = ko.observable(item.Note).extend(Extender.note);
    self.Amount = ko.observable(parseFloat(item.Amount).toFixed(2));
    self.Status = ko.observable(item.Status);
    self.StreetAddress = ko.observable(item.StreetAddress);
    self.CitySub = ko.observable(item.CitySub);
    self.MediaFiles = ko.observableArray(item.MediaFiles);
    self.Files = ko.observableArray(item.Files);
    self.IsEditable = function () {
        return item.Status != 'accepted' && item.Status != 'unsuccessful';
    }
    self.ImgFiles = ko.computed(function () {
        return self.MediaFiles().filter(function (element) {
            return element.MediaType == 1;
        });
    });
    self.DocFiles = ko.computed(function () {
        return self.MediaFiles().filter(function (element) {
            return element.MediaType == 2;
        });
    });
    self.allQuotes = ko.observableArray();
    self.FirstPhoto = ko.computed(function () {
        return KeysFiles.getFirstPhoto(self.MediaFiles());
    });
    self.Note = ko.observable(item.Note).extend(Extender.note);
    self.FilesRemoved = ko.observableArray();
    self.FileWarning = ko.observable(KeysInstrucTion.fileUpLoad);
    self.IsValid = ko.computed(function () {
        var error = ko.validation.group([self.Note]);
        return error().length == 0;
    });

    function MediaViewModel(file) {
        this.Id = file.Id;
        this.File = file.File;
        this.Data = file.Data;
        this.Status = file.Status;
        this.active = file.active;
    }
    self.ImageUrl = ko.observable("");

    self.validFileTypes = [
        "image/jpeg",
        "image/png",
        "image/gif"
    ];

    self.RemoveFile = function (file) {
        if (file.Status === 'load') {
            self.FilesRemoved.push(file.Id);
        }
        self.MediaFiles.remove(file);
    };

    self.imageUpload = function (data, event) {
        var files = event.target.files;
        console.log("the files are" + files);
        var file = files[0];
        var maxFileSize = 5000000;

        for (var i = 0; i < files.length; i++) {
            if (typeof (maxFileSize) !== "undefined" && file.size >= maxFileSize) {
                $("#ImageFile").val("");
                $("#SizeNotSupported").modal("show");
                debugger;
                break;
            }
            if (!~self.validFileTypes.indexOf(file.type)) {
                debugger;
                break;
            }
            else {
                (function (file) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        var result = e.target.result;
                        var doc = {
                            File: file,
                            Data: result,
                            OldFileName: file.name,
                            Status: "add",
                            MediaType: KeysFiles.getType(file)
                        }
                        console.log("file before push" + file);
                        self.MediaFiles.push(new FileModel(doc));

                    };
                    reader.readAsDataURL(file);
                })(files[i]);
            }
        }
    }

    self.documentsUpload = function (data, event) {
        var files = event.target.Files; //file list object
        var file = files[0]
        debugger;
        //loop through each file
        for (var i = 0; i < files.length; i++) {
            // if not image files
            if (!self.validFileTypes.indexOf(file.type)) {
                (function (file) {
                    debugger;
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        var result = e.target.result;
                        var doc = {
                            File: file,
                            Data: result,
                            OldFileName: file.name,
                            Status: "add",
                            MediaType: KeysFiles.getType(file)
                        }
                        debugger;
                        console.log("file before push" + file);
                        self.MediaFiles.push(new FileModel(doc));
                        console.log("file after push" + file);

                    };
                    reader.readAsDataURL(file);
                })(files[i]);
            }
        }
    }

    self.saveChanges = function (data) {
        debugger;
        console.log(data);
        var forSaving = ko.toJSON(data);
        console.log("Media files are" + self.MediaFiles());

        var formData = new FormData();
        formData.append('Note', data.Note());
        formData.append('Id', data.Id());
        var addedPhotos = data.MediaFiles();
        console.log(addedPhotos);
        for (var i = 0; i < addedPhotos.length; i++) {

            formData.append("MediaFiles" + i, addedPhotos[i].File);
        }
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }
        data.FilesRemoved().forEach(function (element) {
            formData.append('FilesRemoved', element);
        });


        $.ajax({
            url: '/Companies/Manage/EditJobQuote',
            type: 'POST',
            dataType: 'json',
            data: formData,// forSaving,
            contentType: false,
            processData: false,
            success: function (result) {

                console.log(result);
                self.allQuotes().forEach(function (element) {
                    if (element.Id() === data.Id()) {
                        var newNote = data.Note();
                        element.Note(newNote);
                        element.MediaFiles(data.MediaFiles());
                    }
                });
                window.location.replace("/Companies/Manage/MyQuotes");

            }
        });
    };
};

var quotesViewModel = function (data) {
    var self = this;
    self.allQuotes = ko.observableArray();
    self.currentQuote = ko.observable();
    data.forEach(function (element) {
        self.allQuotes.push(new quoteModel(element));
    });
    self.showQuoteDetails = function (item) {
        debugger;
        self.isEdit(false);
        self.currentQuote(item);
        $('#mainJobQuotesForm').css('display', 'none');
        $("#jobQuoteDetailsModal").css('display', 'block');
    };
    self.isEdit = ko.observable(false);

    self.showQuoteEdit = function (data) {
        var copy = new quoteModel(ko.toJS(data));
        // Edited by David - 02/11/2017
        //      Set FileWarning of quoteModel selected to empty
        copy.FileWarning('');
        self.currentQuote(copy);
        
        console.log(copy);
        $('#mainJobQuotesForm').css('display', 'none');
        $('#editJobQuoteForm').css('display', 'block');
    };

    self.goToIndex = function () {
        debugger;
        $('#mainJobQuotesForm').css('display', 'block');
        $('#editJobQuoteForm').css('display', 'none');
        $("#jobQuoteDeleteModal").css('display', 'none');
        $("#jobQuoteDetailsModal").css('display', 'none');
        //self.showQuoteDetails(false);
        //self.showQuoteEdit(false);
    }


    self.showJobQuoteDeleteModal = function (item) {
        self.currentQuote(item);
        //$("#jobQuoteDeleteModal").appendTo('body').modal('show');
        $('#mainJobQuotesForm').css('display', 'none');
        $("#jobQuoteDeleteModal").css('display','block');
    };

    self.deleteJobQuote = function () {
        var item = self.currentQuote();
        $('#mainJobQuotesForm').css('display', 'block');
        $("#jobQuoteDeleteModal").css('display', 'none');

        $.ajax({
            url: '/Companies/Manage/DeleteJobQuote',
            type: 'POST',
            dataType: 'json',
            data: JSON.stringify({ jobQuoteid: item.Id() }),
            contentType: 'application/json',
            success: function (result) {
                if (result.success) {                    
                    self.allQuotes.remove(item);
                }
                else {
                    alert("You can not delete this record !!");
                }
             
            }
        });
    };
    self.validFileTypes = [
        "image/jpeg",
        "image/png",
        "image/gif",
        "application/pdf"
    ];
};












