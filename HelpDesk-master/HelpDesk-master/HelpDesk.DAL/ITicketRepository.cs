using HelpDesk.Model;
using System.Collections.Generic;

namespace HelpDesk.DAL
{
    public interface ITicketRepository
    {
        // READ
        List<Ticket> GetAll(string? status = null, int? categoryId = null, string? keyword = null);
        Ticket? Get(int id);
        Ticket? GetById(int id);        // Includes navigation properties
        bool Exists(int id);

        // CREATE
        void Add(Ticket ticket);

        // UPDATE
        bool Update(Ticket ticket);

        // DELETE
        bool Delete(int id);
        void ClearAll();

        // SAVE
        int Save();
    }
}
