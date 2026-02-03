using HelpDesk.BLL;
using HelpDesk.DAL;
using HelpDesk.Model;
using HelpDesk.DTO;

namespace HelpDesk.UI
{
    public partial class Form1 : Form
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketCategoryRepository _ticketCategoryRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public Form1(
            ITicketService ticketService,
            ITicketCategoryRepository ticketCategoryRepository,
            IEmployeeRepository employeeRepository)
        {
            InitializeComponent();
            _ticketService = ticketService;
            _ticketCategoryRepository = ticketCategoryRepository;
            _employeeRepository = employeeRepository;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDefaultValues();
            LoadTickets();
        }

        private void LoadDefaultValues()
        {
            cmbCategory.DataSource = _ticketCategoryRepository.GetAll();
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";

            cmbAssignedTo.DataSource = _employeeRepository.GetAll();
            cmbAssignedTo.DisplayMember = "FullName";
            cmbAssignedTo.ValueMember = "Id";

            cmbStatus.Items.AddRange(new string[] { "New", "In Progress", "Resolved", "Closed" });
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadTickets()
        {
            dgTickets.AutoGenerateColumns = true;
            dgTickets.DataSource = _ticketService.GetAll(null, null, null).ToList();
            dgTickets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgTickets.ReadOnly = true;
            dgTickets.AllowUserToAddRows = false;
        }

        private void btnCreateTicket_Click(object sender, EventArgs e)
        {
            Model.Ticket ticket = new Model.Ticket()
            {
                IssueTitle = txtIssueTitle.Text,
                Description = txtDescription.Text,
                CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
                AssignedEmployeeId = Convert.ToInt32(cmbAssignedTo.SelectedValue),
                Status = cmbStatus.Text
            };

            var result = _ticketService.Add(ticket);

            if (!result.isOk)
                MessageBox.Show(result.message);

            if (result.isOk)
            {
                MessageBox.Show(result.message);
                LoadDefaultValues();
                LoadTickets();
                return;
            }
        }

        private void btnUpdateTicket_Click(object sender, EventArgs e)
        {
            if (dgTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to update.");
                return;
            }

            var selectedTicket = (DTO.Ticket)dgTickets.SelectedRows[0].DataBoundItem;

            Model.Ticket ticket = new Model.Ticket()
            {
                Id = selectedTicket.Id,
                IssueTitle = txtIssueTitle.Text,
                Description = txtDescription.Text,
                CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
                AssignedEmployeeId = Convert.ToInt32(cmbAssignedTo.SelectedValue),
                Status = cmbStatus.Text,
                ResolutionNotes = txtResolutionNotes.Text
            };

            var result = _ticketService.Update(ticket);

            MessageBox.Show(result.message);
            if (result.isOk)
                LoadTickets();

        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (dgTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to delete.");
                return;
            }

            var selectedTicket = (DTO.Ticket)dgTickets.SelectedRows[0].DataBoundItem;

            // Confirm deletion if checkbox checked
            if (chkConfirmDelete.Checked)
            {
                var confirm = MessageBox.Show(
                    "Are you sure you want to delete this ticket?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo);

                if (confirm != DialogResult.Yes)
                    return;
            }

            var result = _ticketService.Delete(selectedTicket.Id);

            MessageBox.Show(result.message);
            if (result.isOk)
                LoadTickets();

        }

        private void btnClearAll_Click_1(object sender, EventArgs e)
        {
            var allTickets = _ticketService.GetAll(null, null, null);
            if (!allTickets.Any())
            {
                MessageBox.Show("No tickets to clear.");
                return;
            }

            var confirm = MessageBox.Show(
                "Are you sure you want to delete ALL tickets?",
                "Confirm Clear All",
                MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            var result = _ticketService.ClearAll();
            MessageBox.Show(result.message);
            if (result.isOk)
                LoadTickets();

        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            string? statusFilter = cmbFilterStatus.Text != "All" ? cmbFilterStatus.Text : null;
            int? categoryFilter = cmbFilterCategory.SelectedValue is int id && id != 0 ? id : null;
            string? keyword = btnApplyFilter.Text;

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length > 100)
            {
                MessageBox.Show("Keyword is too long.");
                return;
            }

            var tickets = _ticketService.GetAll(statusFilter, categoryFilter, keyword);
            dgTickets.DataSource = tickets;

        }

        private void btnResetFilter_Click(object sender, EventArgs e)
        {
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterCategory.SelectedIndex = 0;
            btnResetFilter.Text = string.Empty;

            dgTickets.DataSource = _ticketService.GetAll(null, null, null).ToList();
        }

        private void chkConfirmDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (chkConfirmDelete.Checked)
            {
                chkConfirmDelete.Text = "Delete confirmation is ON";
                chkConfirmDelete.ForeColor = Color.DarkRed;
            }
            else
            {
                chkConfirmDelete.Text = "Delete confirmation is OFF";
                chkConfirmDelete.ForeColor = Color.Gray;
            }
        }

        private void cmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated)
                return;

            string? statusFilter = cmbFilterStatus.Text != "All"
                ? cmbFilterStatus.Text
                : null;

            int? categoryFilter = cmbFilterCategory.SelectedValue is int id && id != 0
                ? id
                : null;

            string? keyword = string.IsNullOrWhiteSpace(btnApplyFilter.Text)
                ? null
                : btnApplyFilter.Text.Trim();

            dgTickets.DataSource = _ticketService
                .GetAll(statusFilter, categoryFilter, keyword)
                .ToList();
        }

        private void cmbFilterCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated)
                return;

            string? statusFilter = cmbFilterStatus.Text != "All"
                ? cmbFilterStatus.Text
                : null;

            int? categoryFilter = cmbFilterCategory.SelectedValue is int id && id != 0
                ? id
                : null;

            string? keyword = string.IsNullOrWhiteSpace(btnApplyFilter.Text)
                ? null
                : btnApplyFilter.Text.Trim();

            var tickets = _ticketService.GetAll(statusFilter, categoryFilter, keyword);
            dgTickets.DataSource = tickets.ToList();
        }

        private void dgTickets_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void cmbAssignedTo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
