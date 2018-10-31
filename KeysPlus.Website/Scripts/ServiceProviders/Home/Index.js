

$(function () {
    //Namespace
    var sp = ServiceProviders;

    var jobListData = $("#jobListData").val();
    var jobListDataObject = $.parseJSON(jobListData);
    var action = $("#userActionData").val();
    //sp.removeServerData();
    var propertyData = sp.loadProperty();
    // Instance of jobViewModel
    var vm = new sp.JobViewModel(jobListDataObject, propertyData);
    sp.formatDate(vm.jobList());
    vm.action = sp.assignAction(action);

    // Bind view model to page
    ko.applyBindings(vm);

    // Set page click function
    $("#pagination").on("click", ".pagination a", function (event) {
        event.preventDefault();
        $.get($(this).prop("href"), null, function (response) {
            sp.reloadData(response, vm.jobList);
        });
    });

    vm.isLoaded(true);

    console.log(vm.jobList());
})