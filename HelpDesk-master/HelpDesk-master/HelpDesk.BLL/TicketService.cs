using HelpDesk.DAL;
using HelpDesk.DTO;
using HelpDesk.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.BLL
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public List<DTO.Ticket> GetAll(string? status, int? category, string? keyword)
        {
            return _ticketRepository
                .GetAll(status, category, keyword)
                .Select(m => new DTO.Ticket
                {
                    Id = m.Id,
                    IssueTitle = m.IssueTitle,
                    Description = m.Description,
                    Category = m.Category?.Name,
                    AssignedEmployee = m.AssignedEmployee?.FullName,
                    Status = m.Status,
                    DateCreated = m.DateCreated,
                    DateResolved = m.DateResolved,
                    ResolutionNotes = m.ResolutionNotes
                })
                .ToList();
        }

        public (bool isOk, string message) Add(Model.Ticket ticket)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ticket.IssueTitle))
                    return (false, "Title must not be empty!");

                if (ticket.CategoryId == null || ticket.CategoryId == 0)
                    return (false, "Category must be selected!");

                if (string.IsNullOrWhiteSpace(ticket.Status))
                    return (false, "Status must be selected!");

                ticket.DateCreated = DateTime.Now;

                if (ticket.Status == "Resolved" || ticket.Status == "Closed")
                {
                    if (ticket.AssignedEmployeeId == null)
                        return (false, "Assigned employee is required.");
                    if (string.IsNullOrWhiteSpace(ticket.ResolutionNotes))
                        return (false, "Resolution notes cannot be empty.");

                    ticket.DateResolved = DateTime.Now;

                    if (ticket.DateResolved < ticket.DateCreated)
                        return (false, "DateResolved cannot be earlier than DateCreated.");
                }
                else
                {
                    ticket.DateResolved = null;
                    ticket.ResolutionNotes = null;
                }

                _ticketRepository.Add(ticket);
                _ticketRepository.Save();
                return (true, "Ticket added successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding ticket: {ex.Message}");
            }
        }

        public (bool isOk, string message) Update(Model.Ticket ticket)
        {
            try
            {
                if (!_ticketRepository.Exists(ticket.Id))
                    return (false, "Ticket does not exist or was removed.");

                if (string.IsNullOrWhiteSpace(ticket.IssueTitle))
                    return (false, "Title must not be empty.");

                if (ticket.CategoryId == null || ticket.CategoryId == 0)
                    return (false, "Category must be selected.");

                var validStatuses = new[] { "New", "In Progress", "Resolved", "Closed" };
                if (!validStatuses.Contains(ticket.Status))
                    return (false, "Invalid status value.");

                // Handle resolving a ticket
                if (ticket.Status == "Resolved" || ticket.Status == "Closed")
                {
                    if (ticket.AssignedEmployeeId == null)
                        return (false, "Assigned employee must be selected to resolve ticket.");

                    if (string.IsNullOrWhiteSpace(ticket.ResolutionNotes))
                        return (false, "Resolution notes must not be empty.");

                    ticket.DateResolved = DateTime.Now;

                    if (ticket.DateResolved < ticket.DateCreated)
                        return (false, "DateResolved cannot be earlier than DateCreated.");
                }
                else
                {
                    ticket.DateResolved = null;
                    ticket.ResolutionNotes = null;
                }

                bool updated = _ticketRepository.Update(ticket);
                if (!updated)
                    return (false, "Failed to update ticket. It may have been removed.");

                _ticketRepository.Save();
                return (true, "Ticket updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating ticket: {ex.Message}");
            }
        }

        public (bool isOk, string message) Delete(int ticketId)
        {
            try
            {
                if (!_ticketRepository.Exists(ticketId))
                    return (false, "Ticket does not exist or was already removed.");

                bool deleted = _ticketRepository.Delete(ticketId);
                if (!deleted)
                    return (false, "Failed to delete ticket.");

                _ticketRepository.Save();
                return (true, "Ticket deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting ticket: {ex.Message}");
            }
        }

        public (bool isOk, string message) ClearAll()
        {
            try
            {
                var tickets = _ticketRepository.GetAll();
                if (!tickets.Any())
                    return (true, "No tickets to clear.");

                _ticketRepository.ClearAll();
                _ticketRepository.Save();
                return (true, "All tickets cleared successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error clearing tickets: {ex.Message}");
            }
        }
    }
}
