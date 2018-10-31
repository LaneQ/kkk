var Company = function (company) {
    var self = this;
    var editUrl = $("#editUrl").val();
    var deleteUrl = $("#deleteUrl").val();
    var createUrl = $("#addUrl").val();
    var uploadImageUrl = $("#ImageUrl").val();
    var companyField = {};
    var PhotoProfile;
    var ImageUrl;
  
    ko.bindingHandlers.cancelEditCompany = {
        init: function (elem, value, allComp, model, context) {
            $(elem).on("click", function () {
                self.companyTemplate("companyIndex");
            });
        }
    };


    $(".body-content").on("click", ".pagedList a", getPage);


    ko.bindingHandlers.AddCss = {
        init: function (elem) {
            $(elem).on("load", function () {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            });
        },
        update: function (elem, value) {
            var val = ko.unwrap(value());

            if (!val.isShipSame()) {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            }
        }
    };

    function reloadPage() {
        // window.location.reload()
        var url = window.location.href;
        if (url.indexOf('?') == -1) {
            url = url + '?';
            location = '?';
            location.reload(true);

        }
    }

    //*************************************** Modal Popup - functions ********************************

    //************ Add company - functions *************

    self.showUploadLogoForCompanyModal = function (data) {
        self.selectedCompany(data);
        $('#UploadLogoForCompany').modal('show');
        $('#UploadLogoForCompany').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    self.showSaveCompanyWithoutLogoModal = function (data) {
        self.selectedCompany(data);
        $('#UploadLogoForCompany').modal('hide');
        $('#SaveCompanyWithoutLogo').modal('show');
        $('#SaveCompanyWithoutLogo').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    self.showSaveCompanyWithLogoModal = function (data) {
        self.selectedCompany(data);
        $('#SaveCompanyWithLogo').modal('show');
        $('#SaveCompanyWithLogo').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    self.showDiscardCompanyDetailsModal = function () {
        $('#DiscardCompanyDetails').modal('show');
        $('#DiscardCompanyDetails').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    //************ Edit company - functions *************

    self.showUpdateCompanyDetailsModal = function (data) {
        self.selectedCompany(data);
        $('#UpdateCompanyDetails').modal('show');
        $('#UpdateCompanyDetails').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    self.showCancelCompanyModalModal = function () {
        $('#CancelCompanyModal').modal('show');
        $('#CancelCompanyModal').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
    }

    //************ Delete company - functions *************

    self.showCompanyDeleteModal = function (data) {
        self.selectedCompany(data);
        $("#DeleteCompanyconfirmation").modal("show");
        $('#DeleteCompanyconfirmation').on('shown.bs.modal', function () {
            $(this).find("[autofocus]:first").focus();
        })
        self.deletedCompany(data);
        console.log(data);
    };

    //************** Discard all changes for modal *********************

    self.cancelCompanyFinally = function (data) {
        restData(data);
        self.company(null);
        self.selectedCompany(null);
        self.company(new CompanyModel(companyField));
        self.companyTemplate("companyIndex");
    };






    //************** Create Company only without logo *********************

    ko.bindingHandlers.saveCompanyOnly = {

        init: function (elem, value, allComp, model, context) {
            var accessor = ko.unwrap(value());

            $(elem).on("click", function () {
                var forSaving = ko.toJSON(accessor);

                if (accessor.Id() === 0) {

                    $.ajax({
                        type: 'post',
                        url: createUrl,
                        data: forSaving,
                        dataType: 'json',
                        contentType: 'application/json;charset=utf-8',
                        success: function (result) {
                            console.log("Company Details Saved");
                            context.$data.Id(result.data.Id);
                            $("#SaveCompanyWithoutLogo").modal("hide");
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            console.log("New Company Created");
                            $("#SavedCompanySuccessfully").modal("show");
                            $('#SavedCompanySuccessfully').on('shown.bs.modal', function () {
                                $(this).find("[autofocus]:first").focus();
                            })
                            context.$parent.companies.push(new CompanyModel(result.data));
                            self.company(null);
                            self.company(new CompanyModel(companyField));
                            return self.companyTemplate("companyIndex");
                        },
                        error: function () { },
                        fail: function () { }
                    });
                }
                else {
                    // console.log(accessor.PhotoProfile());
                    $.ajax({
                        url: editUrl,
                        type: "POST",
                        data: forSaving,
                        contentType: 'application/json;charset=utf-8',
                        dataType: 'json',
                        success: function (result) {
                            ko.mapping.fromJS(result, {}, context.$data);
                            $("#UpdateCompanyDetails").modal("hide");
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            $("#UpdatedCompanySuccessfully").modal("show");
                            $('#UpdatedCompanySuccessfully').on('shown.bs.modal', function () {
                                $(this).find("[autofocus]:first").focus();
                            })
                            console.log("Company details Updated");
                            return self.companyTemplate("companyIndex");
                        },
                        error: function () { },
                        fail: function () { }
                    });
                }
                return false;
            });
        }
    };


    //************** Update Company Logo only  *********************

    ko.bindingHandlers.saveCompanyLogoOnly = {
        init: function (elem, value, allComp, model, context) {
            var accessor = ko.unwrap(value());
            console.log(accessor);
            $(elem).on("click", function (event) {
                var formData = new FormData();
                var addedPhotos = accessor.ImageUrl;
                console.log(addedPhotos);
                formData.append("Files", addedPhotos);
                $.ajax({
                    type: 'post',
                    url: uploadImageUrl + "/" + accessor.Id(),
                    data: formData,
                    dataType: 'json',
                    contentType: false,
                    processData: false,
                    success: function () {
                        console.log("image saved");
                        $("#UpdateCompanyDetails").modal("hide");
                        $("#UpdatedCompanySuccessfully").modal("show");
                        $('body').removeClass('modal-open');
                        $('.modal-backdrop').remove();
                        return self.companyTemplate("companyIndex");
                    },
                    error: function () {
                    },
                    fail: function () {
                    }
                });
                return false;
            });
        }
    };

    //************** Create Company with logo *********************

    ko.bindingHandlers.saveCompanyAndImage = {

        init: function (elem, value, allComp, model, context) {
            var accessor = ko.unwrap(value());
            console.log(accessor);

            $(elem).on("click", function (event) {
                var forSaving = ko.toJSON(accessor);
                var formData = new FormData();
                var addedPhotos = accessor.ImageUrl;
                formData.append("Files", addedPhotos);
                if (accessor.Id() === 0) {
                    $.ajax({
                        type: 'post',
                        url: createUrl,
                        data: forSaving,
                        dataType: 'json',
                        contentType: 'application/json;charset=utf-8',
                        success: function (result) {
                            console.log("Company details Saved");
                            context.$data.Id(result.data.Id);
                            $.ajax({
                                type: 'post',
                                url: uploadImageUrl + "/" + accessor.Id(),
                                data: formData,
                                dataType: 'json',
                                contentType: false,
                                processData: false,
                                success: function () {
                                    console.log("Company Logo Saved");
                                },
                                error: function () {
                                },
                                fail: function () {
                                }
                            });
                            $("#SaveCompanyWithLogoModal").modal("hide");
                            self.disablePhoto(false);
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            $("#SavedCompanySuccessfully").modal("show");
                            $('#SavedCompanySuccessfully').on('shown.bs.modal', function () {
                                $(this).find("[autofocus]:first").focus();
                            })
                            console.log("New Company Created");
                            context.$parent.companies.push(new CompanyModel(result.data));
                            self.company(null);
                            self.company(new CompanyModel(companyField));
                            return self.companyTemplate("companyIndex");
                        },
                        error: function () {
                        },
                        fail: function () {
                        }
                    });
                }
                else {
                    // console.log(accessor.PhotoProfile());
                    $.ajax({
                        url: editUrl,
                        type: "POST",
                        data: forSaving,
                        contentType: 'application/json;charset=utf-8',
                        dataType: 'json',
                        success: function (result) {
                            console.log(result);
                            ko.mapping.fromJS(result, {}, context.$data);
                            $.ajax({
                                type: 'post',
                                url: uploadImageUrl + "/" + accessor.Id(),
                                data: formData,
                                dataType: 'json',
                                contentType: false,
                                processData: false,
                                success: function () {
                                    console.log("image saved");
                                },
                                error: function () {
                                },
                                fail: function () {
                                }
                            });
                            $("#UpdateCompanyDetails").modal("hide");
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            $("#UpdatedCompanySuccessfully").modal("show");
                            $('#UpdatedCompanySuccessfully').on('shown.bs.modal', function () {
                                $(this).find("[autofocus]:first").focus();
                            })
                            console.log("Company details and logo Updated");
                            return self.companyTemplate("companyIndex");
                        },
                        error: function () { },
                        fail: function () { }
                    });
                }
            });
            return false;
        }
    }

    //************** Delete Company *********************

    self.deleteCompany = function (data) {
        console.log(data);
        $.ajax({
            url: deleteUrl,
            type: 'post',
            data: { id: data.Id() }
        }).done(function (result) {
            console.log(result);
            if (result.success) {
                $("#DeletedCompanySuccessfully").modal("show");
                $('#DeletedCompanySuccessfully').on('shown.bs.modal', function () {
                    $(this).find("[autofocus]:first").focus();
                })
                self.companies.remove(function (company) {
                    return company.viewModel.Id() == result.id;
                });
            }
        });
    };

    ko.bindingHandlers.saveCompany = {

        init: function (elem, value, allComp, model, context) {
            var accessor = ko.unwrap(value());

            $(elem).on("click", function () {
                var forSaving = ko.toJSON(accessor);

                if (accessor.Id() === 0) {

                    $.ajax({
                        type: 'post',
                        url: createUrl,

                        data: forSaving,
                        dataType: 'json',
                        contentType: 'application/json;charset=utf-8',
                        success: function (result) {
                            context.$data.Id(result.data.Id);
                            $("#photoUpload").collapse('show');
                            $("#AddPopUp").modal("hide");
                            $("#AddPopUpforNextButton").modal("show");
                            $('#uploadImage').prop("disabled", true);
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();


                            context.$parent.companies.push(new CompanyModel(result.data));
                            self.company(null);
                            self.company(new CompanyModel(companyField));



                        }
                    });

                }
                else {
                    console.log(accessor.PhotoProfile());
                    $.ajax({
                        url: editUrl,
                        type: "POST",
                        data: forSaving,
                        contentType: 'application/json;charset=utf-8',
                        dataType: 'json',
                        success: function (result) {


                            console.log(result);
                            ko.mapping.fromJS(result, {}, context.$data);
                            $("#NextButton").modal("hide");
                            $("#uploadImage").collapse('show');
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            return self.companyTemplate("companyIndex");
                        }
                    });

                }
                return false;
            });
        }
    };

    ko.bindingHandlers.saveImage = {
        init: function (elem, value, allComp, model, context) {
            var accessor = ko.unwrap(value());
            console.log(accessor);
            if (accessor.Id() === 0) {
                $(elem).on("click", function (event) {
                    var formData = new FormData();

                    var addedPhotos = accessor.ImageUrl;

                    formData.append("Files", addedPhotos);

                    $.ajax({
                        type: 'post',
                        url: uploadImageUrl + "/" + accessor.Id(),
                        data: formData,
                        dataType: 'json',
                        contentType: false,
                        processData: false,
                        success: function () {
                            self.company(new CompanyModel(companyField));

                            $("#Savelogo").modal("hide");
                            $("#uploadImage").collapse('hide');
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            self.disablePhoto(false);
                            return self.companyTemplate("companyIndex");


                        },
                        error: function () {
                        },
                        fail: function () {
                        }
                    });

                    return false;
                });

            }
            else {
                console.log(accessor);
                $(elem).on("click", function (event) {
                    console.log("from in side Image save click function");

                    var formData = new FormData();

                    var addedPhotos = accessor.ImageUrl;
                    console.log(addedPhotos);

                    formData.append("Files", addedPhotos);

                    $.ajax({
                        type: 'post',
                        url: uploadImageUrl + "/" + accessor.Id(),
                        data: formData,
                        dataType: 'json',
                        contentType: false,
                        processData: false,
                        success: function () {
                            console.log("image saved");
                            $("#Savelogo").modal("hide");
                            $("#uploadImage").collapse('hide');
                            $('body').removeClass('modal-open');
                            $('.modal-backdrop').remove();
                            $('#UplImage').prop("disabled", true);
                            $('#btndisable').prop("disabled", false);
                            self.disablePhoto(false);
                        },
                        error: function () {
                        },
                        fail: function () {
                        }
                    });
                    return false;
                });
            }
        }
    };

    //************** Checking any changes in add company model *********************

    //self.updateCompanyDetails = function (data) {
    //    var isChanged = ko.utils.unwrapObservable(data.dirty());
    //    var isToggled = ko.utils.unwrapObservable(data.toggleDisable());
    //    if (isChanged == true && isToggled == false) {
    //        self.companyDetailsChanged(true);
    //    }
    //    $('#UpdateCompanyDetails').modal('show');
    //};


    //self.showCompanyEditModal = function (data) {
    //    //if (event.keyCode == 13) {
    //    //    self.saveCompany;
    //    //}
    //    var isChanged = ko.utils.unwrapObservable(data.dirty());
    //    var isToggled = ko.utils.unwrapObservable(data.toggleDisable());

    //    if (isChanged == true && isToggled == false) {
    //        $('#cancelCompanyModal').modal('show');
    //    }
    //    else {
    //        self.company(new CompanyModel(companyField));
    //        self.companyTemplate("companyIndex");
    //    }
    //};


    self.selectedCompany = ko.observable();
    self.deletedCompany = ko.observable();
    self.atIndex = ko.observable(true);
    self.companyTemplate = ko.observable("companyIndex");
    self.pickedTemplate = ko.observable();
    self.companies = ko.observableArray();
    self.company = ko.observable();
    self.disablePhoto = ko.observable(false);
    company.forEach(function (item) {
        self.companies.push(new CompanyModel(item));
        return self.companyTemplate("companyIndex");
    });

    (function () {

        companyField = {
            Id: 0,
            isActive: true,
            Name: "",
            Website: "",
            PhotoProfile: "",
            PhoneNumber: "",
            PhysicalAddress: {
                Address: "",
                CountryId: 1,
                AddressId: 0,
                AddressCont: "",
                Suburb: "",
                City: "",
                PostCode: "",
                Latitude: "",
                Longitude: ""
            },
            BillingAddress: {
                Address: "",
                CountryId: 1,
                AddressId: 0,
                AddressCont: "",
                Suburb: "",
                City: "",
                PostCode: "",
                Latitude: "",
                Longitude: ""
            }
        };
        self.company(new CompanyModel(companyField));
    })();

    function getPage() {
        var anchor = $(this);
        $.ajax({
            url: anchor.attr("herf"),
            type: "get"
        }).done(function (result) {
            console.log(result);
            var target = anchor.parents("#compModel");
            console.log(result);
            target.replaceWith(result);
            self.companies.destroyAll();
            var compList = $.parseJSON($("#compModel").val());
            console.log(compList);
            compList.forEach(function (item) {
                self.companies.push(new CompanyModel(item));
                return self.companyTemplate("companyIndex");

            })
        });
        return false;
    }





    self.viewDetails = ko.computed({
        read: function () {
            return self.companyTemplate();
        },
        write: function (data) {
            self.selectedCompany(data);
            self.atIndex(false);
            return self.companyTemplate("companyDetails");
        }
    });
    self.goToUpLoadPhoto = ko.computed({

        read: function () {

        },
        write: function (data) {
            $("#UploadLogoForCompany").modal("hide");
            $("#companyDetails").collapse('hide');
            $("#uploadImage").collapse('show');
            self.disablePhoto(true);
            $('body').removeClass('modal-open');
            $('.modal-backdrop').remove();
        }
    });

    self.goToIndex = ko.computed({
        read: function () {

            $('body').removeClass('modal-open');
            return self.companyTemplate("companyIndex");
        },
        write: function (data) {
            self.atIndex(true);
            data = ko.mapping.fromJS(data.copy, data.mapOptions, data);
            self.company(null);
            self.company(new CompanyModel(companyField));
            $('body').removeClass('modal-open');

            return self.companyTemplate("companyIndex");
        }
    });
    ko.bindingHandlers.AddCss = {
        init: function (elem) {
            $(elem).on("click", function () {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            });
        },
        update: function (elem, value) {
            var val = ko.unwrap(value());

            if (!val.isShipSame()) {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            }
        }
    };
    self.editCompany = ko.computed({
        read: function () {
            return self.companyTemplate();
        },
        write: function (data) {
            self.selectedCompany(data);
            data.dirty(false);
            self.atIndex(false);
            return self.companyTemplate("CompanyForm");
        }
    });

    self.addCompany = ko.computed({
        read: function () {
            self.disablePhoto(false);
            return self.companyTemplate();
        },
        write: function (data) {
            self.selectedCompany(data);
            self.atIndex(false);
            self.disablePhoto(false);
            return self.companyTemplate("AddCompany");
        }
    });
    //self.checkChange = function (data) {
    //    var isChanged = ko.utils.unwrapObservable(data.dirty);
    //    if (isChanged == true) {
    //        //if (confirm("You have made some changes. Do you wish to save them?") == true) {
    //            $.ajax({
    //                url: editUrl,
    //                type: "POST",
    //                data: ko.toJSON(data),
    //                contentType: 'application/json;charset=utf-8',
    //                dataType: 'json',
    //                success: function (result) {
    //                    console.log(result);
    //                    //getPage();
    //                }
    //            });
    //        }
    //    //    else {
    //    //        //getPage();
    //    //        return self.companyTemplate("companyIndex");
    //    //    }
    //    //}
    //    else {
    //        return self.companyTemplate("companyIndex");
    //    }
    //};
    function restData(datax) {

        datax.selectedCompany._latestValue.Name(datax.selectedCompany._latestValue.copy.Name);
        datax.selectedCompany._latestValue.Website._latestValue = datax.selectedCompany._latestValue.copy.Website;
        datax.selectedCompany._latestValue.PhoneNumber._latestValue = datax.selectedCompany._latestValue.copy.PhoneNumber;
    };




    /*View Model*/

    function CompanyModel(item) {

        var self = this;
        console.log(item);
        var mapOptions = {
            "ignore": ["CreatedBy", "UpdatedBy", "UpdatedOn"],

        };
        var dirtyObservable;

        ko.mapping.defaultOptions().include = [];

        ko.validation.init({
            grouping: {
                deep: true,
                observable: true,
                live: true
            }
        });

        ko.extenders.dirtyTrack = function (target, dirtyObservable) {
            target.subscribe(function () {
                dirtyObservable(true);

            });
            return target;
        };

        ko.validation.registerExtenders();
        self.viewModel = ko.mapping.fromJS(item, mapOptions);
        self.viewModel.copy = item;
        self.viewModel.mapOptions = mapOptions;
        self.validationModel = ko.validatedObservable(self.viewModel);
        self.viewModel.disableItem = ko.observable(true);
        self.viewModel.disableImg = ko.observable(true);
        self.viewModel.dirty = ko.observable(false);
        self.viewModel.UploadStatus = ko.observable(true);

        self.viewModel.companyLogoChanged = ko.observable(false);

        self.viewModel.Id = ko.observable(item.Id);
        self.viewModel.Name = ko.observable(item.Name).extend({ dirtyTrack: self.viewModel.dirty });
        self.viewModel.PhoneNumber = ko.observable(item.PhoneNumber).extend({ dirtyTrack: self.viewModel.dirty });
        self.viewModel.Website = ko.observable(item.Website).extend({ dirtyTrack: self.viewModel.dirty });
        self.viewModel.ImageProfile = ko.observable(item.PhotoProfile).extend({ dirtyTrack: self.viewModel.dirty });
        self.viewModel.PhotoProfile = ko.observable(item.PhotoProfile);
        self.ImageUrl = ko.observable("");
        //self.viewModel.HasCompanyLogo = ko.observable(false);

        self.viewModel.PhysicalAddress = ko.observable(ProjectKeys.Address.EditAddress(item.PhysicalAddress)).extend({ dirtyTrack: self.viewModel.dirty });
        self.viewModel.BillingAddress = ko.observable(ProjectKeys.Address.EditAddress(item.BillingAddress)).extend({ dirtyTrack: self.viewModel.dirty });

        //self.viewModel.ImageFileTypes = [".jpg", ".png", ".gif", ".jpeg"];

        //validations rules

        ko.validation.rules['url'] = {
            validator: function (val, required) {
                // result = false;
                var test = val;

                test = test.replace(/^\s+|\s+$/, ''); //Strip whitespace
                var s = test.toLowerCase()

                var res = s.match(/(http(s)?:\/\/.)?(www\.)?[-a-zA-Z0-9:%._\+~#=]{2,259}\.[a-z0-9]{2,6}\b([-a-zA-Z0-9:%_\+.~#?&//=]*)/g);

                if (res == null)
                    return false;
                else
                    return true;
            },
            message: 'Please enter a valid URL'
        };
        ko.validation.rules['streetLength'] = {
            validator: function (val, required) {

                val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
                if (val.length > 100) {
                    return false;
                } else {
                    return true;
                }
            },
            message: 'Street must be between 1-100 characters'
        };
        ko.validation.rules['numberLength'] = {
            validator: function (val, required) {

                val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
                if (val > 99999 || val < 0) {
                    return false;
                } else {
                    return true;
                }
            },
            message: 'Invalid entry. Must be between 1-99999 '
        };
        ko.validation.rules['urlLength'] = {
            validator: function (val, required) {

                val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
                if (val.length > 250) {
                    return false;
                } else {
                    return true;
                }
            },
            message: 'Invalid entry. Must be between 1-250 characters'
        };
        ko.validation.rules['notzero'] = {
            validator: function (val, required) {

                val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
                if (val == 0) {
                    return false;
                } else {
                    return true;
                }
            },
            message: 'Invalid entry. Input cannot be zero'
        };
        ko.validation.rules['notzero'] = {
            validator: function (val, required) {

                val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
                if (val == 0) {
                    return false;
                } else {
                    return true;
                }
            },
            message: 'Invalid entry. Input cannot be zero'
        };

        ko.validation.rules['numberformat'] = {
            validator: function (val, required) {
                var res = val.match(/^[0-9]+[\s/,&0-9A-Za-z]*$/);
                return res;
            },
            message: 'Please check the format of your number',
        };

        ko.validation.rules['maxnumber'] = {

            validator: function (val, required) {
                debugger;
                if (val.indexOf('/') != -1) {
                    var a = val.indexOf('/');
                    var num = val.substring(0, a);
                    if (num > 9999) {
                        return false;
                    } else {
                        return true;
                    }
                }
            },
            message: 'please enter number between 1-9999',
        };
        ko.validation.rules['streetformat'] = {
            validator: function (val, required) {
                var res = val.match(/^[ A-Za-z0-9,-./]*$/);
                return res;
            },
            message: 'Please check the street contains alphanumeric and special characters [,-./]',
        };
        ko.validation.rules['cityformat'] = {
            validator: function (val, required) {
                var res = val.match(/^[ A-Za-z\s]*$/);
                return res;
            },
            message: 'City only accepts alphabtic characters and spaces',
        };

        self.validationModel = ko.validatedObservable({
            Name: self.viewModel.Name.extend({
                required: {
                    params: true,
                    message: "Please enter a company name."
                },
                pattern: {
                    params: "^[A-Za-z0-9][A-Za-z0-9\\s\/\\,\\.\\-\\_\\~\\`\\@\\#\\$\\&\\*\\;\\:\]{1,199}$",
                    message: "Enter a valid input and maximum 200 characters only"
                },
            }),
            Website: self.viewModel.Website.extend({
                required: {
                    params: true, message: "Please enter a Website address."
                },

                url: true,
                urlLength: true
            }),
            PhoneNumber: self.viewModel.PhoneNumber.extend({
                required: {
                    params: true,
                    message: "Please enter a Contact number."
                },
                maxLength: {
                    params: 11,
                    message: "Invalid entry. Must be between 9-11 digits.",


                },
                minLength: {
                    params: 9,
                    message: "Invalid entry. Must be between 9-11 digits."

                },
                digit: {
                    params: true,
                    message: "Please enter a valid contact number."
                },
            }),
            PhysicalAddress: self.viewModel.PhysicalAddress().Number.extend({
                required: {
                    params: true,
                    message: "Please enter your number."
                },

                numberformat: true,
            }),

            PhysicalAddressCont: self.viewModel.PhysicalAddress().Street.extend({
                required: {
                    params: true,
                    message: "Please include your Street."
                },
                streetformat: true
            }),
            PhysicalSuburb: self.viewModel.PhysicalAddress().Suburb.extend({
                required: {
                    params: false,
                    message: "Please include your Suburb."
                },
                pattern: {
                    params: "^[ A-Za-z\s]*$",
                    message: "Suburb accepts alpabetic values only"
                },


            }),
            PhysicalPostCode: self.viewModel.PhysicalAddress().PostCode.extend({
                required: {
                    params: true,
                    message: "Please include your Post Code."
                },
                Number: {
                    params: true,

                },
                pattern: {
                    params: "^[0-9]{1,}$",
                    message: "Post Code accepts numeric values only"
                },
                notzero: true,
            }),
            PhysicalCity: self.viewModel.PhysicalAddress().City.extend({
                required: {
                    params: true,
                    message: "Please include your City."
                },
                //pattern: {
                //    params: "^[A-Za-z ]{1,50}$",
                //    message: "The City field must be alphabetic characters."
                //},
                cityformat: true


            }),

            BillingAddress: self.viewModel.BillingAddress().Number.extend({
                required: {
                    params: true,
                    message: "Please enter your number."
                },

                numberformat: true,

            }),
            BillingAddressCont: self.viewModel.BillingAddress().Street.extend({
                required: {
                    params: true,
                    message: "Please include your Street."
                },
                //pattern: {

                //    params: "^[A-Za-z0-9][A-Za-z\\s\-\\,\\/\\&\]{0,100}$",
                //    message: "The Street field Must be between 1-100 characters."
                //},
                streetformat: true
            }),
            BillingSuburb: self.viewModel.BillingAddress().Suburb.extend({
                required: {
                    params: false,
                    message: "Please include your Suburb."
                },
                pattern: {
                    params: "^[ A-Za-z\s]*$",
                    message: "Suburb accepts alpabetic values only"
                },
            }),
            BillingPostCode: self.viewModel.BillingAddress().PostCode.extend({
                required: {
                    params: true,
                    message: "Please include your Post Code."
                },
                Number: {
                    params: true,

                },
                pattern: {
                    params: "^[0-9]{1,}$",
                    message: "Post Code accepts numeric values only"
                },
                notzero: true,

            }),
            BillingCity: self.viewModel.BillingAddress().City.extend({
                required: {
                    params: true,
                    message: "Please include your City."
                },
                //pattern: {
                //    params: "^[A-Za-z ]{1,50}$",
                //    message: "The City field must be alphabetic characters."
                //},
                cityformat: true
            })
        });
        self.viewModel.clearValidations = function reset() {
            self.viewModel.Name(self.viewModel.copy.Name);
            self.viewModel.PhotoProfile(self.viewModel.copy.PhotoProfile);
            self.viewModel.Website(self.viewModel.copy.Website);
            self.viewModel.Name.isModified(false);
            self.viewModel.Website.isModified(false);
        };
        self.viewModel.fullAddress = ko.computed(function () {
            var val = item.PhysicalAddress.Number + " " + item.PhysicalAddress.Street + " " + item.PhysicalAddress.Suburb + " "
                + item.PhysicalAddress.City + " " + item.PhysicalAddress.PostCode;
            return val;
        });

        self.viewModel.toggleDisable = ko.computed({
            read: function () {
                if (!self.validationModel.isValid()) {
                    self.validationModel.errors.showAllMessages();
                    self.viewModel.disableItem(true);
                }
                else {
                    self.viewModel.disableItem(false);
                }
                return self.viewModel.disableItem();
            }
        });

        self.viewModel.toggleImgDisable = ko.computed({
            read: function () {
                if (!self.viewModel.ImageProfile()) {
                    self.viewModel.disableImg(true);
                }
                else {
                    self.viewModel.disableImg(false);                   
                }        
                return self.viewModel.disableImg();
            }
        });

        self.viewModel.isExpanded = ko.observable(false);

        self.viewModel.toggle = function () {
            self.viewModel.isExpanded(!self.viewModel.isExpanded());
        };
        //self.viewModel.isShipSame = ko.observable(false);
        console.log(item.isShipSame);
        if (item.isShipSame == true) {
            console.log("is ship  same");
            self.viewModel.isShipSame = ko.observable(item.isShipSame);
            self.viewModel.checksame = 1;
            console.log(item.isShipSame);

        }
        else {
            console.log("is ship not same");
            self.viewModel.isShipSame = ko.observable(false);

        }

        //clear all billing address when its uncheck.

        self.viewModel.isShipDifferent = ko.computed(function () {
            if (self.viewModel.isShipSame()) {
                $.each(self.viewModel.BillingAddress(), function (item, value) {
                    if (self.viewModel.BillingAddress()[item].hasOwnProperty("isModified")) {
                        self.viewModel.BillingAddress()[item].isModified(false);
                    }
                    self.viewModel.BillingAddress()[item](self.viewModel.PhysicalAddress()[item]());
                });
            }
            else {
                $.each(self.viewModel.BillingAddress(), function (item, value) {
                    if (self.viewModel.BillingAddress()[item].hasOwnProperty("isModified")) {
                        self.viewModel.BillingAddress()[item].isModified(false);
                    }
                    self.viewModel.BillingAddress()[item](self.viewModel.copy.BillingAddress[item]);
                    $('#billingAddress').find('.clearAutoAddress').val('');
                });
            }
            return !self.viewModel.isShipSame();
        });

        self.viewModel.validFileTypes = [
            "image/jpeg",
            "image/png",
            "image/gif"
        ];

        self.viewModel.imageUpload = function (data, event) {
            var files = event.target.files;
            console.log(files);
            var file = files[0];
            var maxFileSize = 5000000;

            //self.validationModel.isValid(true);

            for (var i = 0; i < files.length; i++) {

                if (typeof (maxFileSize) != "undefined" && file.size >= maxFileSize) {
                    $("#ImageFile").val("");
                    $("#SizeNotSupported").modal("show");
                    break;
                }
                if (!~self.viewModel.validFileTypes.indexOf(file.type)) {
                    if (self.viewModel.ImageProfile().length <= 0) {
                       // self.validationModel.isValid(false);
                    }
                    $("#Unsupported").modal("show");
                    $('#uploadImage').prop("disabled", true);
                    self.viewModel.UploadStatus(true);
                    $("#ImageFile").val("");
                    break;
                }
                else {

                    (function (file) {

                        var reader = new FileReader();
                        reader.onload = function (e) {
                            var result = e.target.result;
                            $('#uploadImage').prop("disabled", false);
                            self.viewModel.UploadStatus(false);
                            $('#btndisable').prop("disabled", true);
                            self.viewModel.companyLogoChanged(true);
                            image = {
                                File: file,
                                Data: result,
                                Status: "add"
                            }
                            self.viewModel.PhotoProfile(file.name);
                            self.viewModel.ImageProfile(result);

                            self.viewModel.ImageUrl = file;
                            console.log(self.viewModel.ImageUrl);
                            $('#UplImage').prop("disabled", false);
                            $('#uploadImage').prop("disabled", false);
                            $('#btndisable').prop("disabled", true);
                        };
                        reader.readAsDataURL(file);
                    })(file);

                }
            }

        }
        self.viewModel.ResetAddress = ko.computed({

            read: function () {
                if (self.viewModel.isShipSame()) {
                    $.each(self.viewModel.BillingAddress(), function (key, value) {

                        if (key === "AddressId") {
                            return true;
                        }
                        self.viewModel.BillingAddress()[key](self.viewModel.PhysicalAddress()[key]());

                        if (self.viewModel.BillingAddress()[key].hasOwnProperty("isModified")) {
                            self.viewModel.BillingAddress()[key].isModified(false);
                        }
                    });
                }
                return self.viewModel.BillingAddress();
            },
            write: function () {

                self.viewModel.isShipSame(!self.viewModel.isShipSame());
            }
        });
        self.viewModel.removeImage = function (item) {
            $('#uploadImage').prop("disabled", false);
            $("#ImageFile").val("");
            self.viewModel.ImageProfile(null);
            self.viewModel.PhotoProfile(null);
        };

    }
};
