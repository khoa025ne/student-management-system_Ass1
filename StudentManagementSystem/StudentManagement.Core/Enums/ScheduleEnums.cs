namespace StudentManagement.Core.Enums
{
    // Cặp ngày: 2-5, 3-6, 4-7
    public enum DayOfWeekPair
    {
        MonThu = 1, // 2-5
        TueFri = 2, // 3-6
        WedSat = 3  // 4-7
    }

    // Slot trong ngày
    public enum TimeSlot
    {
        Slot1 = 1,
        Slot2 = 2,
        Slot3 = 3,
        Slot4 = 4
    }

    // Ngày đơn, nếu sau này cần show chi tiết
    public enum WeekDay
    {
        Monday = 1, // Thứ 2
        Tuesday = 2, // Thứ 3
        Wednesday = 3, // Thứ 4
        Thursday = 4, // Thứ 5
        Friday = 5, // Thứ 6
        Saturday = 6  // Thứ 7
    }
}
