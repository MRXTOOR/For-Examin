using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable dataTable;

        public Form1()
        {
            InitializeComponent();

            // Инициализация подключения к базе данных
            string connectionString = @"Server = DESKTOP-2MGOE61; Database = cars; Integrated Security = True";
            connection = new SqlConnection(connectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "carsDataSet.Cars". При необходимости она может быть перемещена или удалена.
            this.carsTableAdapter.Fill(this.carsDataSet.Cars);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "jobAgencyDataSet.Jobs". При необходимости она может быть перемещена или удалена.
            this.jobsTableAdapter.Fill(this.jobAgencyDataSet.Jobs);
            // Загрузка данных с базы данных и отображение их в DataGridView
            LoadData();
        }

        private void LoadData()
        {
            string query = "SELECT * FROM Cars";
            adapter = new SqlDataAdapter(query, connection);

            // Создание команды вставки
            SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();

            dataTable = new DataTable();
            adapter.Fill(dataTable);
            dataGridView.DataSource = dataTable;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Обновление данных на форме из базы данных
            LoadData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Добавление новой записи в DataGridView и базу данных
            // Проверка наличия записи с таким же значением в колонке VacancyCode
            int vacancyCode;
            if (!Int32.TryParse(txtCode.Text, out vacancyCode))
            {
                MessageBox.Show("Введите корректное значение для CarCode.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool vacancyExists = dataTable.AsEnumerable().Any(row => row.Field<int>("CarCode") == vacancyCode);
            if (vacancyExists)
            {
                MessageBox.Show("Такая вакансия уже существует.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRow newRow = dataTable.NewRow();
            newRow["CarCode"] = vacancyCode;
            newRow["BrandName"] = txtTitle.Text;
            newRow["Color"] = comboType.Text;
            newRow["ManufactureYear"] = decimal.Parse(txtSalary.Text);
            dataTable.Rows.Add(newRow);
            adapter.Update(dataTable);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Удаление записи из DataGridView и базы данных
            int vacancyCodeToDelete;
            if (!int.TryParse(txtCode.Text, out vacancyCodeToDelete))
            {
                MessageBox.Show("Введите корректное значение для CarCode.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var matchingRows = dataTable.AsEnumerable().Where(row => row.Field<int>("CarCode") == vacancyCodeToDelete).ToList();
            if (matchingRows.Count == 0)
            {
                MessageBox.Show("Запись с таким CarCode не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var row in matchingRows)
                {
                    dataTable.Rows.Remove(row);
                }

                adapter.Update(dataTable);
                MessageBox.Show("Запись успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTitle = txtSearchTitle.Text;
            string searchSalary = txtSearchSalary.Text;

            if (searchTitle == "" && searchSalary == "")
            {
                MessageBox.Show("Введите значение для поиска.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataTable filteredDataTable = dataTable.Clone(); // Создаем копию таблицы с теми же столбцами

            foreach (DataRow row in dataTable.Rows)
            {
                string vacancyTitle = row.Field<string>("BrandName");
            
                int salary = row.Field<int>("ManufactureYear");

                if ((searchTitle == "" || vacancyTitle.IndexOf(searchTitle, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (searchSalary == "" || int.TryParse(searchSalary, out int searchSalaryValue) && salary >= searchSalaryValue))
                {
                    filteredDataTable.ImportRow(row);
                }
            }

            dataGridView.DataSource = filteredDataTable;
        }
    }
}
