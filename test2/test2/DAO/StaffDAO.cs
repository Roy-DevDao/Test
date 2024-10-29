using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using test2.Data;

namespace test2.DAO
{
    public class StaffDAO
    {
        private readonly DocCareContext dc;

        public StaffDAO(DocCareContext context)
        {
            dc = context;
        }

        public bool UpdateAppointmentStatus(string appointmentId, string newStatus = "Completed")
        {
            using (var transaction = dc.Database.BeginTransaction())
            {
                try
                {
                    var order = dc.Options.FirstOrDefault(o => o.OptionId == appointmentId);
                    if (order != null)
                    {
                        order.Status = newStatus;
                        dc.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error updating appointment status: {ex.Message}");
                    return false;
                }
            }
        }



    }
}