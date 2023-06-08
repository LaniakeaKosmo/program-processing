using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

using Microsoft.Office.Interop.Excel;
//using DataTable = Microsoft.Office.Interop.Excel.DataTable;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        IExcelDataReader edr;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Content();
        }


        private void Button_List_Patient(object sender, RoutedEventArgs e)
        {
            WpfAppPatients taskWindow = new WpfAppPatients();
            taskWindow.Show();
        }

        private void Button_Directions(object sender, RoutedEventArgs e)
        {
            WpfAppDirections taskWindow = new WpfAppDirections();
            taskWindow.Show();
        }

        private void Button_Direction_Files(object sender, RoutedEventArgs e)
        {
            WpfAppDirectionFiles taskWindow = new WpfAppDirectionFiles();
            taskWindow.Show();
        }

        //показ изображения из файла
        public class Command : ICommand
        {
            public Command(System.Action action)
            {
                this.action = action;
            }

            System.Action action;

            EventHandler canExecuteChanged;
            event EventHandler ICommand.CanExecuteChanged
            {
                add { canExecuteChanged += value; }
                remove { canExecuteChanged -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                action();
            }
        }

        public class Content : INotifyPropertyChanged
        {
            public Content()
            {
                openFileDialogCommand = new Command(ExecuteOpenFileDialog);
                openFileDialog = new OpenFileDialog()
                {
                    Multiselect = true,
                    Filter = "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
                };
            }

            readonly OpenFileDialog openFileDialog;

            public ImageSource Image { get; private set; }

            readonly ICommand openFileDialogCommand;
            public ICommand OpenFileDialogCommand { get { return openFileDialogCommand; } }

            void ExecuteOpenFileDialog()
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                    {
                        Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        RaisePropertyChanged("Image");
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            void RaisePropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //Выгрузка таблицы 1

        private void OpenExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXCEL Files (*.xlsx)|*.xlsx|EXCEL Files 2003 (*.xls)|*.xls|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != true)
                return;

            var data1 = readFile(openFileDialog.FileName);
            var columnsList = new List<columnData>();
            foreach (DataColumn columns in data1.Table.Columns)
            {
                var column = new columnData(columns.Ordinal, columns.ColumnName);
                columnsList.Add(column);
            }
            var index = columnsList.Find(x => x.columnName == "Fio_d").columnIndex;
            foreach (DataRow row in data1.Table.Rows)
            {
                var row1 = row.ItemArray[index];
            }
            DbGrig.ItemsSource = data1;
        }

        private DataView readFile(string fileNames)
        {

            var extension = fileNames.Substring(fileNames.LastIndexOf('.'));
            // Создаем поток для чтения.
            FileStream stream = File.Open(fileNames, FileMode.Open, FileAccess.Read);
            // В зависимости от расширения файла Excel, создаем тот или иной читатель.
            // Читатель для файлов с расширением *.xlsx.
            if (extension == ".xlsx")
                edr = ExcelReaderFactory.CreateOpenXmlReader(stream);
            // Читатель для файлов с расширением *.xls.
            else if (extension == ".xls")
                edr = ExcelReaderFactory.CreateBinaryReader(stream);

            //// reader.IsFirstRowAsColumnNames
            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };
            // Читаем, получаем DataView и работаем с ним как обычно.
            DataSet dataSet = edr.AsDataSet(conf);
            DataView dtView = dataSet.Tables[0].AsDataView();

            // После завершения чтения освобождаем ресурсы.
            edr.Close();
            return dtView;
        }







        public void MainWindow_Load(object sender, EventArgs e)
        {
            DataTable table = LoadExcelSheetToTable(@"D:\Primer.xlsx", "Primer");
            PlaceTableToDatabase(table);
        }

        private void PlaceTableToDatabase(DataTable table)
        {
            ModelDB db = new ModelDB();
            table.PrimaryKey = new DataColumn[] { table.Columns["Fio"] };
            foreach (DataRow row in table.Rows)
            {
                string Fio = Convert.ToString(row["Fio"]);
                Primer primer = db.Primer.Find(Fio);
                if (primer == null)
                {
                    primer = new Primer();
                    primer.Fio = Fio;
                    primer.Fio_d = Convert.ToString(row["Fio_d"]);
                    primer.Numberib = Convert.ToInt32(row["Numberib"]);
                    primer.Date_gosp = Convert.ToDateTime(row["Date_gosp"]);
                    primer.Date_vipis = Convert.ToDateTime(row["Date_vipis"]);
                    primer.Otdel = Convert.ToString(row["Otdel"]);
                    primer.Address = Convert.ToString(row["Address"]);
                    primer.Type_gosp = Convert.ToInt32(row["Type_gosp"]);
                    primer.Polic = Convert.ToString(row["Polic"]);
                    primer.Type_pay = Convert.ToString(row["Type_pay"]);
                    db.Primer.Add(primer);
                }
                else
                {
                    primer.Fio = Fio;
                    primer.Fio_d = Convert.ToString(row["Fio_d"]);
                    primer.Numberib = Convert.ToInt32(row["Numberib"]);
                    primer.Date_gosp = Convert.ToDateTime(row["Date_gosp"]);
                    primer.Date_vipis = Convert.ToDateTime(row["Date_vipis"]);
                    primer.Otdel = Convert.ToString(row["Otdel"]);
                    primer.Address = Convert.ToString(row["Address"]);
                    primer.Type_gosp = Convert.ToInt32(row["Type_gosp"]);
                    primer.Polic = Convert.ToString(row["Polic"]);
                    primer.Type_pay = Convert.ToString(row["Type_pay"]);
                }
            }
            //foreach (Primer primer in db.Primer)
            //    if (table.Rows.Find(primer.Fio) == null) db.Primer.Remove(primer);
            db.SaveChanges();
        }

        private DataTable LoadExcelSheetToTable(string filename, string sheet)
        {
            DataTable dtImport = new DataTable();
            using (System.Data.OleDb.OleDbConnection co =
                new System.Data.OleDb.OleDbConnection(
                    "Provider=Microsoft.ACE.OLEDB.12.0;" +
                    "Data Source=" + filename + ";" +
                    "Extended Properties=\"Excel 12.0 Xml;HDR=YES\";"))


            using (System.Data.OleDb.OleDbDataAdapter import =
               new System.Data.OleDb.OleDbDataAdapter(
                    "select * from [" + sheet + "$]", co))

                import.Fill(dtImport);

            return dtImport;
        }
    }


    public class columnData
    {
        public int columnIndex;
        public string columnName;

        public columnData(int columnInd, string columnNam)
        {
            columnIndex = columnInd;
            columnName = columnNam;
        }
    }

}
