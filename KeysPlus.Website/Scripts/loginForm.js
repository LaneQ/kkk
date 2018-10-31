var viewModel = function () {
    var self = this;  
    self.myBoolean = ko.observable(false);
    self.errorMessage = ko.observable("");

    self.toggleMyBoolean = function () { 
      
        self.myBoolean(true);        
        $("input").prop('checked', true);
    }

    self.checkVariable = function () {
        if (self.myBoolean() == false) {
            return self.errorMessage("Please read the terms of usage!");
        }
        else {
            return true;
        }
    }
}
ko.applyBindings(new viewModel());


$('.toggle').on('click', function () {
    $('.container').stop().addClass('active');
});

$('.close').on('click', function () {
    $('.container').stop().removeClass('active');
});
$(document).ready(function () {
    var dropDown = $('#roleDropdown');
    $('#dropdown-input').find('.bootstrap-select').remove();
    $('#dropdown-input').append(dropDown);
});
//(function ($) {
//    "use strict";

//    // Options for Message
//    //----------------------------------------------
//    var options = {
//        'btn-loading': '<i class="fa fa-spinner fa-pulse"></i>',
//        'btn-success': '<i class="fa fa-check"></i>',
//        'btn-error': '<i class="fa fa-remove"></i>',
//        'msg-success': 'All Good! Redirecting...',
//        'msg-error': 'Wrong login credentials!',
//        'useAJAX': true,
//    };

//    // Login Form
//    //----------------------------------------------
//    // Validation
//    $("#login-form").validate({
//        rules: {
//            UserName: "required",
//            Password: "required",
//        },
      
//        errorClass: "form-invalid"
//    });

//    // Form Submission
//    $("#login-form").submit(function () {
//        remove_loading($(this));

//        if (options['useAJAX'] === true) {
//            dummy_submit_form($(this));
//            $.ajax({
//                url: myUrl,
//                type: "POST",
//                data: { UserName: userName, Password:password },
            
//                //data: JSON.stringify(data),
//                dataType: "json",
//                contentType: "application/json",
//                success: function (status) {
//                    if (status.success) {
//                        setTimeout(function () {
//                            form_success($form);
//                            window.location.href = status.TargetURL;
//                        }, 2000);
//                        //window.location.href = status.TargetURL;

//                    }
//                    else {
//                        form_failed($form);
//                    }
//                        //   window.location.href = "@Url.Action("Login", "Account")";
//              },  
//                error: function () {
//                    form_failed($form);
//                    console.log('Login Fail!!!');
//                }
//            });







//            //////$.ajax({
//            //////    url: "/Account/Login",
//            //////    type: "POST",
//            //////    data: JSON.stringify(data),
//            //////    dataType: "json",
//            //////    contentType: "application/json",
//            //////    success: function (status) {
//            //////        if (status.Success) {
//            //////            window.location.href = status.TargetURL;
                      
//            //////        }
//            //////    //    else
//            //////            //   window.location.href = "@Url.Action("Login", "Account")";
//            //////  }  
//            //////    //error: function () {
//            //////    //    console.log('Login Fail!!!');
//            //////    //}
//            //////});

//            // Dummy AJAX request (Replace this with your AJAX code)
//            // If you don't want to use AJAX, remove this
//            //dummy_submit_form($(this));

//            // Cancel the normal submission.
//            // If you don't want to use AJAX, remove this
//            return false;
//        }
//    });
//    // Loading
//    //----------------------------------------------
//    function remove_loading($form) {
//        $form.find('[type=submit]').removeClass('error success');
//        $form.find('.login-form-main-message').removeClass('show error success').html('');
//    }

//    function form_loading($form) {
//        $form.find('[type=submit]').addClass('clicked').html(options['btn-loading']);
//    }

//    function form_success($form) {
//        $form.find('[type=submit]').addClass('success').html(options['btn-success']);
//        $form.find('.login-form-main-message').addClass('show success').html(options['msg-success']);

//    }

//    function form_failed($form) {
//        $form.find('[type=submit]').addClass('error').html(options['btn-error']);
//        $form.find('.login-form-main-message').addClass('show error').html(options['msg-error']);
//    }

//    // Dummy Submit Form (Remove this)
//    //----------------------------------------------
//    // This is just a dummy form submission. You should use your AJAX function or remove this function if you are not using AJAX.
//    function dummy_submit_form($form) {
//        if ($form.valid()) {
//            form_loading($form);

//            //setTimeout(function () {
//            //    form_success($form);
//            //}, 2000);
//        }
//    }

//})(jQuery);