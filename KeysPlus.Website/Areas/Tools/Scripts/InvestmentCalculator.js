var fortnightsInYear = 26.0714285;
var weeksInYear = 52.142857;
function PropertyValue() {
    var self = this;
    self.EstimatedValue = ko.observable();
    self.CapitalGain = ko.observable();
}
function RentalIncome() {
    var self = this;
    self.WeeklyRentalIncome = ko.observable();
    self.VacancyRate = ko.observable();
    self.AnnualRentalIncome = ko.pureComputed(function () {
        if (!self.VacancyRate()) {
            return self.WeeklyRentalIncome() * weeksInYear;
        }
        return self.WeeklyRentalIncome() * weeksInYear - self.VacancyRate() * self.WeeklyRentalIncome();
    });
}
function Expenses() {
    var self = this;
    self.Frequencies = [{ Code: weeksInYear, Name: 'Weekly' }, { Code: fortnightsInYear, Name: 'Fortnightly' }, { Code: 12, Name: 'Monthly' }, { Code: 6, Name: 'Quarterly' }, { Code: 2, Name: 'Biannually' }, { Code: 1, Name: 'Annually' }];
    self.CouncilRateFreq = ko.observable(self.Frequencies[0]);
    self.MaintenanceFreq = ko.observable(self.Frequencies[0]);
    self.InsuranceFreq = ko.observable(self.Frequencies[0]);
    self.OtherFreq = ko.observable(self.Frequencies[0]);
    self.CouncilRateExp = ko.observable().extend(Extender.calcDecimalNumeric);
    self.MaintenanceExp = ko.observable().extend(Extender.calcDecimalNumeric);
    self.InsuranceExp = ko.observable().extend(Extender.calcDecimalNumeric);
    self.OtherExp = ko.observable().extend(Extender.calcDecimalNumeric);
    self.IsValid = ko.computed(function () {
        errors = ko.validation.group(self.CouncilRateExp);
        return errors().length === 0;
    });
    self.AnnualExp = ko.computed(function () {
        var sum = 0;
        if (self.CouncilRateExp()) {
            if (isValuePositiveNumber(self.CouncilRateExp())) {
                sum = sum + self.CouncilRateFreq().Code * self.CouncilRateExp();
            }
        }
        if (self.MaintenanceExp()) {
            if (isValuePositiveNumber(self.MaintenanceExp())) {
                sum = sum + self.MaintenanceFreq().Code * self.MaintenanceExp();
            }
        }
        if (self.InsuranceExp()) {
            if (isValuePositiveNumber(self.InsuranceExp())) {
                sum = sum + self.InsuranceFreq().Code * self.InsuranceExp();
            }
        }
        if (self.OtherExp()) {
            if (isValuePositiveNumber(self.OtherExp())) {
                sum = sum + self.OtherFreq().Code * self.OtherExp();
            }
        }
        return sum;
    });
    // Expense has to be a number and can't be negative
    function isValuePositiveNumber(value) {
        if (isNaN(value)) {
            return false;
        }
        else if (value >= 0) {
            return true;
        }
        return false;
    }
}
function LoanRepayments() {
    var self = this;

    self.InterestYearOne = ko.observable();
    self.PrincipalYearOne = ko.observable();
    self.LoanAmount = ko.observable();
    self.InterestRate = ko.observable();
    self.LoanType = [{ Name: "Interest only" }, { Name: "Principal and interest" }];
    self.LoanTypeSelected = ko.observable(self.LoanType[0]);
    self.RepaymentFrequencies = [{ Code: 12, Name: 'Monthly' }, { Code: fortnightsInYear, Name: 'Fortnightly' }];
    self.RepaymentFreqSelected = ko.observable(self.RepaymentFrequencies[0]);
    self.LoanTermYears = ko.observable();
    self.LoanTermMonths = ko.observable();
    self.IsInterestOnly = ko.computed(function () {
        if (self.LoanTypeSelected().Name === "Interest only") {
            self.InterestYearOne(null);
            self.PrincipalYearOne(null);
            self.LoanTermMonths(null);
            self.LoanTermYears(null);
            return true;
        }
        else {
            return false;
        }
    });
    self.LoanTermYearsToMonths = ko.computed(function () { // function to convert loan term from a decimal to year and month
        if (self.LoanTermYears() % 1 === 0) { // is loan term years a whole number?
            self.LoanTermMonths(null);
        }
        else if (self.LoanTermYears()) {
            var remainderInYears = self.LoanTermYears() % 1; // Remainder to calc months with
            var remainderInMonths = Math.round(remainderInYears * 12);// convert remainder in months to display
            if (remainderInMonths === 12) {
                self.LoanTermYears(Math.round(self.LoanTermYears() - remainderInYears + 1));
                self.LoanTermMonths(null);
            }
            else {
                self.LoanTermYears(self.LoanTermYears() - remainderInYears);// adjust loan term years
                if (remainderInMonths === 0) {
                    self.LoanTermMonths(null); // leave input box empty if months are 0
                }
                else {
                    self.LoanTermMonths(remainderInMonths);
                }
            }
        }
    });
    self.RepaymentAmount = ko.observable();
    self.LoanTermGivenRepaymentAmount = ko.computed(function () {
        if (self.LoanAmount() && self.InterestRate() && self.LoanTypeSelected().Name === "Interest only") {
            var interestRate = self.InterestRate() / 100;
            var result = (self.LoanAmount() * interestRate) / self.RepaymentFreqSelected().Code; // Monthly or fortnightly repayment interest calc
            self.RepaymentAmount(result.toFixed(2));
        }
        else if (self.LoanAmount() && self.InterestRate() && self.LoanTypeSelected().Name === "Principal and interest") {
            var interestRateMonthly = self.InterestRate() / 100 / 12; // Make interest rate a decimal and convert to monthly
            var interestRateFortnightly = self.InterestRate() / 100 / fortnightsInYear; // Make interest rate a decimal and convert to fortnightly
            var principal = self.LoanAmount();
            var loanTermMonth = 0;
            if (self.LoanTermMonths()) { // Calculate loan term

                if (self.LoanTermYears()) {
                    var monthsConversion = self.LoanTermMonths();
                    var yearsConversion = self.LoanTermYears() * 12;
                    loanTermMonth = parseFloat(yearsConversion) + parseFloat(monthsConversion);
                }
                else {
                    loanTermMonth = self.LoanTermMonths();
                }
            }
            else {
                if (self.LoanTermYears()) {
                    loanTermMonth = self.LoanTermYears() * 12;
                }
                else {
                    self.InterestYearOne(null);
                    self.PrincipalYearOne(null);
                    self.RepaymentAmount(null);
                    return false;
                }
                var x = Math.pow(1 + interestRateMonthly, loanTermMonth); //compute powers for annuity payment calcualtion
                var monthly = (principal * x * interestRateMonthly) / (x - 1); //annuity payment calculation for monthly
                var fortnightly = (principal * x * interestRateFortnightly) / (x - 1); //annuity payment calculation for fortnightly
                var interestRateDecimal = self.InterestRate() / 100; //find interest rate decimal per annum
                var PrincipalYearOne = getPrincipalYearOne(); // Calcualte the amount of principal paid off in one year, monthly and fortnightly
                function getPrincipalYearOne() {
                    var sum = 0;
                    var count = 0;
                    var diminishingPrincipal = principal;
                    var principalMonthly = 0;
                    var principalFortnightly = 0;
                    if (self.RepaymentFreqSelected().Name === "Monthly") { // MONTHLY
                        while (count < 12) {
                            principalMonthly = monthly - (diminishingPrincipal * interestRateDecimal / 12); // subtracting off the monthly interest each month from the total monthly payment,
                            diminishingPrincipal = diminishingPrincipal - principalMonthly;                 // the sum of the principal payments gives the total principal paid in year one

                            sum = sum + principalMonthly;
                            count = count + 1;
                            principalMonthly = 0;
                        }
                        return Math.round(sum);
                    }
                    else if (self.RepaymentFreqSelected().Name === "Fortnightly") { // FORTNIGHTLY Same principle as the monthly but with fortnightly payments
                        while (count < 26) {
                            principalFortnightly = fortnightly - (diminishingPrincipal * interestRateDecimal / fortnightsInYear);
                            diminishingPrincipal = diminishingPrincipal - principalFortnightly;


                            sum = sum + principalFortnightly;
                            count = count + 1;
                            principalFortnightly = 0;
                        }
                        return Math.round(sum);
                    }
                }

                if (self.RepaymentFreqSelected().Name === "Monthly") {
                    if (!self.LoanTermYears() && self.LoanTermMonths() < 12) { // Stop displaying principal year one and interest year one if the loan term is less than one year
                        self.PrincipalYearOne(null);
                        self.InterestYearOne(null);
                        if (!self.RepaymentAmount()) {
                            self.RepaymentAmount(monthly.toFixed(2));
                        }
                    }
                    else {
                        self.InterestYearOne(Math.round((monthly * 12) - PrincipalYearOne)); // display principle year one and interest year one if loan term is greater than one year
                        self.PrincipalYearOne(Math.round(PrincipalYearOne));
                        if (!self.RepaymentAmount() || self.LoanTermYears() || self.LoanTermMonths()) {
                            self.RepaymentAmount(monthly.toFixed(2));
                        }
                    }

                }
                else if (self.RepaymentFreqSelected().Name === "Fortnightly") {
                    if (!self.LoanTermYears() && self.LoanTermMonths() < 12) {
                        self.PrincipalYearOne(null);
                        self.InterestYearOne(null);
                        if (!self.RepaymentAmount()) {
                            self.RepaymentAmount(fortnightly.toFixed(2));
                        }
                    }
                    else {
                        if (self.RepaymentAmount() && !self.LoanTermYears() && !self.LoanTermMonths()) {
                            self.InterestYearOne(Math.round((self.RepaymentAmount() * fortnightsInYear) - PrincipalYearOne));
                        }
                        else {
                            self.InterestYearOne(Math.round((fortnightly.toFixed(2) * fortnightsInYear) - PrincipalYearOne));
                        }
                        self.PrincipalYearOne(Math.round(PrincipalYearOne));
                        if (!self.RepaymentAmount() || self.LoanTermYears() || self.LoanTermMonths()) {
                            self.RepaymentAmount(fortnightly.toFixed(2));
                        }
                    }
                }
            }
        }
    });
    self.AnnualLoanRepayment = ko.computed(function () {
        if (self.LoanAmount() && self.InterestRate() && self.LoanTypeSelected().Name === "Interest only") { // Interest only calculation
            var interestRate = self.InterestRate() / 100; // Make interest rate a decimal
            var result = self.LoanAmount() * interestRate;
            return result;
        }
        else if (self.RepaymentAmount() && self.LoanTypeSelected().Name === "Principal and interest") { // Principal and interest calculations
            if (!self.LoanAmount()) {
                self.RepaymentAmount(null); // resets fields if loan amount is deleted
            }
            if (self.RepaymentFreqSelected().Name === "Monthly") {
                if (!self.LoanTermYears() && self.LoanTermMonths() < 12) {
                    return Math.round(self.RepaymentAmount() * self.LoanTermMonths()); // if loan term is less than a year than calcualte annual loan
                    // payments based on amount of months
                }
                else {
                    return Math.round(self.RepaymentAmount() * 12); // otherwise annual loan repayment is equal to Repayment Amount * 12 (one year)
                }
            }
            else if (self.RepaymentFreqSelected().Name === "Fortnightly") {
                if (!self.LoanTermYears() && self.LoanTermMonths() < 12) {
                    return Math.round(self.RepaymentAmount() * (self.LoanTermMonths() * 2)); // if loan term is less than a year than calcualte annual loan
                }                                                                            // payments based on amount of months
                else {
                    return Math.round(self.RepaymentAmount() * fortnightsInYear); // otherwise annual loan repayment is equal to Repayment Amount * 26.07 (one year)
                }
            }
        }
    });
    self.IsInterestRate = ko.computed(function () { // checks if
        if (!self.InterestRate()) {
            self.RepaymentAmount(null);
            return false;
        }
        else {
            return true;
        }
    });
    function calcLoanTerm() {
        if (self.LoanAmount() && self.InterestRate() && self.LoanTypeSelected().Name === "Principal and interest" && self.RepaymentAmount()) {
            var principal = self.LoanAmount();
            if (self.RepaymentFreqSelected().Name === 'Monthly') {
                var repaymentAmount = self.RepaymentAmount();
                var interestRateMonthly = self.InterestRate() / 100 / 12;
                var monthsLoanTerm = Math.log(repaymentAmount / (repaymentAmount - principal * interestRateMonthly)) / Math.log(1 + interestRateMonthly);
                var yearsLoanTerm = monthsLoanTerm / 12;
                self.LoanTermYears(yearsLoanTerm);
            }
            else if (self.RepaymentFreqSelected().Name === 'Fortnightly') {
                var repaymentAmountF = self.RepaymentAmount();
                var interestRateFortnightly = self.InterestRate() / 100 / fortnightsInYear;
                var fortnightlyLoanTerm = Math.log(repaymentAmountF / (repaymentAmountF - principal * interestRateFortnightly)) / Math.log(1 + interestRateFortnightly);
                var yearsLoanTermF = fortnightlyLoanTerm / fortnightsInYear;
                self.LoanTermYears(yearsLoanTermF);
            }
        }
    }
    self.IsRepaymentAboveMin = ko.computed(function () { // prevents error from payment being to low (interest and principal can't be less than interest only)
        if (self.RepaymentAmount() && self.LoanTypeSelected().Name === 'Principal and interest') {
            if (self.RepaymentFreqSelected().Name === 'Monthly') {
                var interestPayment = self.LoanAmount() * (self.InterestRate() / 100 / 12);
                if (self.RepaymentAmount() < (interestPayment + 1)) {
                    $('#repaymentMessage').show();
                    self.RepaymentAmount(interestPayment + 1);
                    calcLoanTerm();
                }
                else {
                    $('#repaymentMessage').hide();
                    calcLoanTerm();
                }
            }
            else if (self.RepaymentFreqSelected().Name === 'Fortnightly') {
                var interestPaymentFortnightly = self.LoanAmount() * (self.InterestRate() / 100 / fortnightsInYear);
                if (self.RepaymentAmount() < (interestPaymentFortnightly + 1)) {
                    $('#repaymentMessage').show();
                    self.RepaymentAmount((interestPaymentFortnightly + 1).toFixed(2));
                    calcLoanTerm();
                }
                else {
                    $('#repaymentMessage').hide();
                    if (!self.LoanTermYears() && !self.LoanTermMonths()) {
                        calcLoanTerm();
                    }
                }
            }
        }
        else {
            if (!self.RepaymentAmount()) {
                self.LoanTermYears(null);
                self.LoanTermMonths(null);
                self.InterestYearOne(null);
                self.PrincipalYearOne(null);
            }
            $('#repaymentMessage').hide();
            calcLoanTerm();
        }
    });
}
function InvestmentCalculatorViewModel() {
    var self = this;
    self.PropertyValue = new PropertyValue();
    self.RentalIncome = new RentalIncome();
    self.Expenses = new Expenses();
    self.LoanRepayments = new LoanRepayments();

    // TAX
    self.TaxRates = [{ Code: 0, Name: "0 %" }, { Code: 0.105, Name: "10.5 %" }, { Code: 0.175, Name: "17.5 %" }, { Code: 0.30, Name: "30 %" }, { Code: 0.33, Name: "33 %" }];
    self.SelectedTaxRate = ko.observable(self.TaxRates[0]);
    self.AnnualTax = ko.computed(function () {
        var taxableIncome = (self.RentalIncome.AnnualRentalIncome() - self.Expenses.AnnualExp() - self.LoanRepayments.InterestYearOne()); // Calculate annual tax
        var tax = taxableIncome * self.SelectedTaxRate().Code;
        if (tax > 0) {
            tax = -Math.abs(tax);
        }
        else {
            tax = Math.abs(tax);
        }
        return tax;
    });
    // Your estimated return at end of Year 1
    self.GrossRentalYield = ko.computed(function () {
        var rentalYieldDecimal = self.RentalIncome.AnnualRentalIncome() / self.PropertyValue.EstimatedValue();
        var rentalYieldPercent = (rentalYieldDecimal * 100).toFixed(1);
        if (isNaN(rentalYieldPercent)) {
            return 0;
        }
        return rentalYieldPercent;
    });
    self.NetRentalYield = ko.computed(function () {
        var netRentalYieldDecimal = (self.RentalIncome.AnnualRentalIncome() - self.Expenses.AnnualExp()) / self.PropertyValue.EstimatedValue();
        var netRentalYieldPercent = (netRentalYieldDecimal * 100).toFixed(1);
        if (isNaN(netRentalYieldPercent)) {
            return 0;
        }
        return netRentalYieldPercent;
    });
    self.CapitalGainYearOne = ko.computed(function () {
        var capitalGainDecimal = self.PropertyValue.CapitalGain() / 100;
        var capitalGain = capitalGainDecimal * self.PropertyValue.EstimatedValue();
        if (isNaN(capitalGain)) {
            return 0;
        }
        return capitalGain;
    });
    self.AnnualEquity = ko.computed(function () {
        var annualEquity = self.CapitalGainYearOne() + self.LoanRepayments.PrincipalYearOne();
        if (isNaN(annualEquity)) {
            return 0;
        }
        return annualEquity;
    });
    self.AnnualCashFlow = ko.computed(function () {
        var cashFlow = (self.RentalIncome.AnnualRentalIncome() + self.AnnualTax()) - self.Expenses.AnnualExp() - self.LoanRepayments.AnnualLoanRepayment();
        if (isNaN(cashFlow)) {
            return 0;
        }
        return cashFlow;
    });
}
$(function () {
    $('[data-toggle="tooltip"]').tooltip();
});
function showVacancyMessage() {
    var x = document.getElementById("vacancy");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function showGrossRentalYield() {
    var x = document.getElementById("grossRentalYield");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function showNetRentalYield() {
    var x = document.getElementById("netRentalYield");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function showCapitalGain() {
    var x = document.getElementById("capitalGain");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function showAnnualEquity() {
    var x = document.getElementById("annualEquity");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
function showAnnualCashFlow() {
    var x = document.getElementById("annualCashFlow");
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
$(function () {
    ko.applyBindings(new InvestmentCalculatorViewModel());
});