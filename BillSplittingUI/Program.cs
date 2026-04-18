using BillSplitting.Application;
using BillSplitting.Data;
using BillSplittingUI;
using BillSplittingUI.Screens;

var debtRepository = new CsvDebtRepository("debts.csv");
var handler = new RecordDebtHandler(debtRepository);
var paymentRepository = new CsvPaymentRepository("payments.csv");
var paymentHandler = new RecordPaymentHandler(paymentRepository, debtRepository);
var billRepository = new CsvBillRepository("bills.csv");
var billHandler = new CreateBillHandler(billRepository, debtRepository);
var personRepository = new CsvPersonRepository("people.csv");   

var screens = new Dictionary<string, IScreen>
{
    ["Record a Debt"] = new RecordDebtScreen(handler, personRepository),
    ["Record a Payment"] = new RecordPaymentScreen(paymentHandler, debtRepository, personRepository),
    ["Record a Bill"] = new RecordBillScreen(billHandler, personRepository),
    ["View Summaries"] = new ViewSummaryScreen(debtRepository, personRepository, paymentRepository, billRepository),
    ["Test Performance"] = new TestPerformanceScreen(debtRepository, personRepository, paymentRepository, billRepository, "debts.csv", "people.csv", "payments.csv", "bills.csv")
};

var app = new App(screens, debtRepository);
app.Run();