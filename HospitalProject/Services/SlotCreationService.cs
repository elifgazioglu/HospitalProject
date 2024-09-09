using api.Data;
using HospitalProject.Models;

namespace HospitalProject.Services
{
    public class SlotCreationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SlotCreationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                    CreateSlotsForWeek(dbContext);
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }


        private void CreateSlotsForWeek(ApplicationDBContext dbContext)
        {
            var doctors = dbContext.Doctors.ToList();
            var slots = dbContext.Slots.ToList();
            var startDate = new DateTime();
            DateTime? endDate = null;


            if (!slots.Any())
            {
                startDate = DateTime.Today.AddHours(9);
                //endDate = DateTime.Today.AddHours(17);
            }
            else
            {
                var lastSlot = slots.Last();
                startDate = lastSlot.SlotDate.Value.AddDays(1).AddHours(-8).AddMinutes(15);
                endDate = lastSlot.SlotDate.Value.AddDays(1);
            }

            if (doctors.Any())
            {
                if (endDate.HasValue)
                {
                    AddSlots(dbContext, doctors, startDate, endDate.Value);
                }
                else
                {
                    // sd -> 05.09.2024 09:00:00 ed -> 19.09.2024 17:00:00 
                    // sd -> 06.09.2024 17:00:00
                    for (var date = startDate; date <= startDate.AddDays(14); date = date.AddDays(1))
                    {
                        var StartTime = date.Date.AddHours(9);
                        var EndTime = date.Date.AddHours(16).AddMinutes(45);

                        AddSlots(dbContext, doctors, StartTime, EndTime);
                    }

                }

            }
        }

        private static void AddSlots(ApplicationDBContext dbContext, List<Doctor> doctors, DateTime startDate, DateTime endDate)
        {
            foreach (var doctor in doctors)
            {

                for (var date = startDate; date <= endDate; date = date.AddMinutes(15))
                {
                    var slot = new Slot
                    {
                        DoctorId = doctor.Id,
                        SlotDate = date,
                        Status = 1
                    };
                    dbContext.Slots.Add(slot);
                    dbContext.SaveChanges();
                }

            }
        }
    }
}