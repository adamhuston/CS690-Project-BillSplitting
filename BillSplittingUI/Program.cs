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


var screens = new Dictionary<string, IScreen>
{
    ["Record a Debt"] = new RecordDebtScreen(handler),
    ["Record a Payment"] = new RecordPaymentScreen(paymentHandler, debtRepository),
    ["Record a Bill"] = new RecordBillScreen(billHandler),
    ["View Summaries"] = new ViewSummaryScreen()
};

var app = new App(screens, debtRepository);
app.Run();