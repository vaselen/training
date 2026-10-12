using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DanceSchoolApi.Models
{
    public class Subscription
    {
        private Subscription()
        {
        }

        // Сервис проверяет ученика, команду и тариф перед созданием абонемента.
        internal Subscription(int studentId, int teamId, int tariffId, DateOnly purchaseDate, int durationDays)
        {
            if (durationDays <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationDays));

            StudentId = studentId;
            TeamId = teamId;
            TariffId = tariffId;
            PurchaseDate = purchaseDate;
            EndDate = purchaseDate.AddDays(durationDays);
        }

        public int SubscriptionId { get; set; }

        // Кто купил
        public int StudentId { get; private set; }
        public Student Student { get; private set; } = null!;

        // В какую команду
        public int TeamId { get; private set; }
        public Team Team { get; private set; } = null!;

        // По какому тарифу
        public int TariffId { get; private set; }
        public Tariff Tariff { get; private set; } = null!;

        public DateOnly PurchaseDate { get; private set; }

        // Первый день, когда абонемент больше не действует; при отмене — день ухода.
        public DateOnly EndDate { get; private set; }
        public bool IsCancelled { get; private set; } = false;

        [NotMapped]
        public bool IsActiveNow => IsActiveOn(DateOnly.FromDateTime(DateTime.Today));

        [NotMapped]
        public bool IsExpiredByTime => !IsCancelled && EndDate <= DateOnly.FromDateTime(DateTime.Today);

        [NotMapped]
        public int DaysLeft => GetDaysLeft(DateOnly.FromDateTime(DateTime.Today));

        public bool IsActiveOn(DateOnly date) => !IsCancelled && PurchaseDate <= date && date < EndDate;

        public int GetDaysLeft(DateOnly date) => IsActiveOn(date) ? EndDate.DayNumber - date.DayNumber : 0;

        public void Cancel(DateOnly? currentDate = null)
        {
            var today = currentDate ?? DateOnly.FromDateTime(DateTime.Today);

            if (IsCancelled)
                throw new InvalidOperationException("Абонемент уже отменён; восстановление невозможно.");

            if (!IsActiveOn(today))
                throw new InvalidOperationException("Досрочно отменить можно только действующий абонемент.");

            EndDate = today;
            IsCancelled = true;
        }
    }
}
