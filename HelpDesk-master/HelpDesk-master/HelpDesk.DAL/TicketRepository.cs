using HelpDesk.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.DAL
{
    public class TicketRepository : ITicketRepository
    {
        private readonly HelpDeskDbContext _context;

        public TicketRepository(HelpDeskDbContext context)
        {
            _context = context;
        }

        // =========================
        // READ
        // =========================
        public List<Ticket> GetAll(string? status = null, int? categoryId = null, string? keyword = null)
        {
            var tickets = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.AssignedEmployee)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                tickets = tickets.Where(t => t.Status == status);

            if (categoryId.HasValue && categoryId > 0)
                tickets = tickets.Where(t => t.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                tickets = tickets.Where(t =>
                    t.IssueTitle.Contains(keyword) ||
                    (t.AssignedEmployee != null && t.AssignedEmployee.FullName.Contains(keyword)));
            }

            return tickets.ToList();
        }

        public Ticket? Get(int id) => _context.Tickets.Find(id);

        public Ticket? GetById(int id) => _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedEmployee)
            .FirstOrDefault(t => t.Id == id);

        public bool Exists(int id) => _context.Tickets.Any(t => t.Id == id);

        // =========================
        // CREATE
        // =========================
        public void Add(Ticket ticket) => _context.Tickets.Add(ticket);

        // =========================
        // UPDATE
        // =========================
        public bool Update(Ticket ticket)
        {
            // Get the existing entity from the database (tracked by EF)
            var existingTicket = _context.Tickets.Find(ticket.Id);
            if (existingTicket == null)
                return false; // Ticket does not exist or was removed

            // Update only the fields you want
            existingTicket.IssueTitle = ticket.IssueTitle;
            existingTicket.Description = ticket.Description;
            existingTicket.CategoryId = ticket.CategoryId;
            existingTicket.AssignedEmployeeId = ticket.AssignedEmployeeId;
            existingTicket.Status = ticket.Status;
            existingTicket.ResolutionNotes = ticket.ResolutionNotes;
            existingTicket.DateResolved = ticket.DateResolved;

            // EF is tracking existingTicket, so no need to call _context.Tickets.Update
            return true;
        }

        // =========================
        // DELETE
        // =========================
        public bool Delete(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            return true;
        }

        public void ClearAll()
        {
            var tickets = _context.Tickets.ToList();
            if (tickets.Any())
                _context.Tickets.RemoveRange(tickets);
        }

        // =========================
        // SAVE
        // =========================
        public int Save() => _context.SaveChanges();
    }
}
