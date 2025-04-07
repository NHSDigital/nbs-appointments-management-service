namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class MultiServiceTests : AvailabilityCalculationsBase
{
    [Fact]
    public async Task MultipleServicesByCreatedDate()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Green", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("5", "Blue", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("6", "Green", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("7", "Blue", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "10:00", ["Green"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        // Bookings 1, 2, 3, 6 and 7 should be supported
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "6", "7");

        // Bookings 4 and 5 should not be, because they were created after 6 and 7
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        resultingAvailabilityState.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");
    }

    /// <summary>
    ///     Check that if there is only one candidate slot, it finds it early
    /// </summary>
    [Fact]
    public async Task BestFitModel_PerformanceCheck_1()
    {
        var bookings = new List<Booking> { TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1) };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue", "Orange", "Black", "Grey"], capacity: 1),
            TestSession("09:00", "10:00", ["Pink"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");
    }

    /// <summary>
    ///     Check that if there are two candidate slots, it finds it early
    /// </summary>
    [Fact]
    public async Task BestFitModel_PerformanceCheck_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Blue", "Orange", "Black", "Grey", "Purple"], capacity: 1),
            TestSession("09:00", "10:00", ["Pink", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    /// <summary>
    ///     Check that if all the opportunity costs are the same,
    ///     that it prioritises by booking order as secondary sort
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_1()
    {
        //configured in a way where all opportunity cost values are equal!
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Grey", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Black", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Black", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Green", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("9", "Grey", avStatus: "Orphaned", creationOrder: 9),
            TestBooking("10", "Blue", avStatus: "Orphaned", creationOrder: 10),
            TestBooking("11", "Blue", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "Purple", avStatus: "Orphaned", creationOrder: 12),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue", "Orange", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        //only first 3 can fit!!
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    }

    /// <summary>
    ///     Check that if all the opportunity costs are the same
    ///     that it prioritises by booking order as secondary sort
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_2()
    {
        //configured in a way where all opportunity cost values are equal!
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Grey", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Grey", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Black", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Black", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Green", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("8", "Green", avStatus: "Orphaned", creationOrder: 8),
            TestBooking("9", "Green", avStatus: "Orphaned", creationOrder: 9),
            TestBooking("10", "Blue", avStatus: "Orphaned", creationOrder: 10),
            TestBooking("11", "Blue", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "Purple", avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "Grey", avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "Blue", avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "Black", avStatus: "Orphaned", creationOrder: 15),
            TestBooking("15", "Blue", avStatus: "Orphaned", creationOrder: 16),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue", "Orange", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Blue"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        //only first 5 can fit!!
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5");
    }

    /// <summary>
    ///     Check that if all the opportunity costs are the same
    ///     no bookings for some of the offered services
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_3()
    {
        //configured in a way where all opportunity cost values are equal!
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Blue", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Black", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Black", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Grey", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("8", "Green", avStatus: "Orphaned", creationOrder: 8),
            TestBooking("9", "Green", avStatus: "Orphaned", creationOrder: 9),
            TestBooking("10", "Blue", avStatus: "Orphaned", creationOrder: 10),
            TestBooking("11", "Blue", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "Purple", avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "Grey", avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "Blue", avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "Black", avStatus: "Orphaned", creationOrder: 15),
            TestBooking("16", "Grey", avStatus: "Orphaned", creationOrder: 16),
            TestBooking("17", "Green", avStatus: "Orphaned", creationOrder: 17),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue", "Orange", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Blue"], capacity: 1),
            //equal opp.cost but no bookings for these colours
            TestSession("09:00", "09:10", ["Pink", "Lilac", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Yellow", "Maroon", "Blue"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        //only first 7 can fit!!
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7");
    }

    /// <summary>
    ///     Check that if all the opportunity costs are the same
    ///     it prioritises the ones with more bookings coming up!
    /// </summary>
    [Fact(Skip = "Not finished writing")]
    public async Task BestFitModel_EqualOpportunityCost_4()
    {
        //configured in a way where all opportunity cost values are equal!
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Grey", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Black", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Black", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Green", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("8", "Grey", avStatus: "Orphaned", creationOrder: 8),
            TestBooking("9", "Purple", avStatus: "Orphaned", creationOrder: 9),
            TestBooking("10", "Blue", avStatus: "Orphaned", creationOrder: 10),
            TestBooking("11", "Blue", avStatus: "Orphaned", creationOrder: 11),
        };

        for (var i = 0; i < 10; i++)
        {
            bookings.Add(TestBooking($"{12 + i}", "Purple", avStatus: "Orphaned", creationOrder: 12 + i));
        }

        for (var i = 0; i < 10; i++)
        {
            bookings.Add(TestBooking($"{22 + i}", "Blue", avStatus: "Orphaned", creationOrder: 22 + i));
        }

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Black", "Grey", "Blue", "Orange", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Black", "Grey", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            //want to preserve this slot as more bookings coming up?
            TestSession("09:00", "09:10", ["Purple", "Blue"], capacity: 10),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        //only first 3 can fit!!
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    }

    /// <summary>
    ///     Prove that equal opportunity cost metric is not enough, and that it needs something else to sort by
    ///     Try and make the algorithm make 2 sub-optimal decisions, leading to loss of booking
    ///     Verify that oversubscribed services don't impact best fit model
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_5()
    {
        //configured in a way where all opportunity cost values are equal

        //setup so that opp.cost for when evaluating first A booking is 0.5 for each of X,Y,Z (x misses out)
        //setup so that opp.cost for when evaluating the final A booking is 1 for each of P,X (x misses out again)

        //start with single A booking
        var bookings = new List<Booking>
        {
            //first 'decision', takes 'AX' cause all other slots have 50% opp.cost
            TestBooking("1", "A", avStatus: "Orphaned", creationOrder: 1),
        };

        for (var i = 0; i < 5; i++)
        {
            bookings.Add(TestBooking($"{i + 2}", "Y", avStatus: "Orphaned", creationOrder: i + 2));
        }

        for (var i = 0; i < 5; i++)
        {
            bookings.Add(TestBooking($"{i + 7}", "A", avStatus: "Orphaned", creationOrder: i + 7));
        }

        //second 'decision', takes 'AX' cause all other slots have 100% opp.cost
        bookings.Add(TestBooking("12", "A", avStatus: "Orphaned", creationOrder: 12));

        for (var i = 0; i < 10; i++)
        {
            bookings.Add(TestBooking($"{i + 13}", "Z", avStatus: "Orphaned", creationOrder: i + 13));
        }

        //the booking that 'could' miss out
        bookings.Add(TestBooking("23", "X", avStatus: "Orphaned", creationOrder: 23));

        //loads of T bookings to put them at the back of the queue.
        for (var i = 0; i < 100; i++)
        {
            bookings.Add(TestBooking($"{i + 24}", "T", avStatus: "Orphaned", creationOrder: i + 24));
        }

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["T", "A", "X"], capacity: 2),
            TestSession("09:00", "09:10", ["T", "A", "Y"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A", "Z"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A"], capacity: 50)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        var expectedArray = new List<string>();

        //all first 72 bookings should make the cut, and the last 50+ 'T' bookings don't
        for (var i = 0; i < 72; i++)
        {
            expectedArray.Add($"{i + 1}");
        }

        resultingAvailabilityState.Recalculations.Should().HaveCount(expectedArray.Count);
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo(expectedArray);
    }

    /// <summary>
    ///     Verify that oversubscribed services don't impact best fit model
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_6()
    {
        var bookings = new List<Booking>();

        //possibly load up the first 3 sessions with bookings 'A' incorrectly
        for (var i = 0; i < 22; i++)
        {
            bookings.Add(TestBooking($"{i + 1}", "A", avStatus: "Orphaned", creationOrder: i + 1));
        }

        for (var i = 0; i < 5; i++)
        {
            bookings.Add(TestBooking($"{i + 23}", "Y", avStatus: "Orphaned", creationOrder: i + 23));
        }

        for (var i = 0; i < 10; i++)
        {
            bookings.Add(TestBooking($"{i + 28}", "Z", avStatus: "Orphaned", creationOrder: i + 28));
        }

        bookings.Add(TestBooking("38", "X", avStatus: "Orphaned", creationOrder: 38));

        //loads of T bookings to put them at the back of the queue.
        for (var i = 0; i < 72; i++)
        {
            bookings.Add(TestBooking($"{i + 39}", "T", avStatus: "Orphaned", creationOrder: i + 39));
        }

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["T", "A", "X"], capacity: 2),
            TestSession("09:00", "09:10", ["T", "A", "Y"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A", "Z"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A"], capacity: 50)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        var expectedArray = new List<string>();

        //all first 72 bookings should make the cut, and the last 50+ 'T' bookings don't
        for (var i = 0; i < 72; i++)
        {
            expectedArray.Add($"{i + 1}");
        }

        resultingAvailabilityState.Recalculations.Should().HaveCount(expectedArray.Count);
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo(expectedArray);
    }

    /// <summary>
    ///     Prove that equal opportunity cost metric is not enough, and that it needs something else to sort by
    ///     Try and make the algorithm make 2 sub-optimal decisions, leading to loss of booking
    ///     Verify that oversubscribed services don't impact best fit model
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_7()
    {
        //configured in a way where all opportunity cost values are equal

        //setup so that opp.cost for when evaluating first A booking is 0.5 for each of X,Y,Z (x misses out)
        //setup so that opp.cost for when evaluating the final A booking is 1 for each of P,X (x misses out again)

        //start with single A booking
        var bookings = new List<Booking>
        {
            //first 'decision', takes 'AX' cause all other slots have 50% opp.cost
            TestBooking("1", "A", avStatus: "Orphaned", creationOrder: 1),
        };

        for (var i = 0; i < 5; i++)
        {
            bookings.Add(TestBooking($"{i + 2}", "Y", avStatus: "Orphaned", creationOrder: i + 2));
        }

        for (var i = 0; i < 5; i++)
        {
            bookings.Add(TestBooking($"{i + 7}", "A", avStatus: "Orphaned", creationOrder: i + 7));
        }

        //second 'decision', takes 'AX' cause all other slots have 100% opp.cost
        bookings.Add(TestBooking("12", "A", avStatus: "Orphaned", creationOrder: 12));

        for (var i = 0; i < 10; i++)
        {
            bookings.Add(TestBooking($"{i + 13}", "Z", avStatus: "Orphaned", creationOrder: i + 13));
        }

        bookings.Add(TestBooking("23", "T", avStatus: "Orphaned", creationOrder: 23));

        //the booking that 'could' miss out
        bookings.Add(TestBooking("24", "X", avStatus: "Orphaned", creationOrder: 24));

        //loads of T bookings to put them at the back of the queue.
        for (var i = 0; i < 71; i++)
        {
            bookings.Add(TestBooking($"{i + 25}", "T", avStatus: "Orphaned", creationOrder: i + 25));
        }

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["T", "A", "X"], capacity: 2),
            TestSession("09:00", "09:10", ["T", "A", "Y"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A", "Z"], capacity: 10),
            TestSession("09:00", "09:10", ["T", "A"], capacity: 50)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        var expectedArray = new List<string>();

        //all first 72 bookings should make the cut, and the last 50+ 'T' bookings don't
        for (var i = 0; i < 72; i++)
        {
            expectedArray.Add($"{i + 1}");
        }

        resultingAvailabilityState.Recalculations.Should().HaveCount(expectedArray.Count);
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo(expectedArray);
    }

    /// <summary>
    ///     Prove that the model needs to be smart enough to make a correct decision when there are
    ///     multiple permutation candidates to check at the same time, and that it tries to prioritise booking order
    ///     when making a decision
    /// </summary>
    [Fact]
    public async Task BestFitModel_EqualOpportunityCost_8()
    {
        //setup in such a way, that when choosing for first booking for X
        //there could be 4 equally weighted permutation slots to choose from (1-4)
        //it needs to choose the correct one so that not only can it fit as many bookings in as possible in general,
        //that in that decision it still prioritises booking order

        //22 bookings

        //X,T,4A,4B,4C,4D,4T
        var bookings = new List<Booking>
        {
            TestBooking("1", "X", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "T", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "A", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "A", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "A", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "A", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "B", avStatus: "Orphaned", creationOrder: 7),
            TestBooking("8", "B", avStatus: "Orphaned", creationOrder: 8),
            TestBooking("9", "B", avStatus: "Orphaned", creationOrder: 9),
            TestBooking("10", "B", avStatus: "Orphaned", creationOrder: 10),
            TestBooking("11", "C", avStatus: "Orphaned", creationOrder: 11),
            TestBooking("12", "C", avStatus: "Orphaned", creationOrder: 12),
            TestBooking("13", "C", avStatus: "Orphaned", creationOrder: 13),
            TestBooking("14", "C", avStatus: "Orphaned", creationOrder: 14),
            TestBooking("15", "D", avStatus: "Orphaned", creationOrder: 15),
            TestBooking("16", "D", avStatus: "Orphaned", creationOrder: 16),
            TestBooking("17", "D", avStatus: "Orphaned", creationOrder: 17),
            TestBooking("18", "D", avStatus: "Orphaned", creationOrder: 18),
            TestBooking("19", "T", avStatus: "Orphaned", creationOrder: 19),
            TestBooking("20", "T", avStatus: "Orphaned", creationOrder: 20),
            TestBooking("21", "T", avStatus: "Orphaned", creationOrder: 21),
            TestBooking("22", "T", avStatus: "Orphaned", creationOrder: 22),
        };

        //9 slots
        var sessions = new List<SessionInstance>
        {
            //the permutations are ABC,ABD,ACD,BCD
            TestSession("09:00", "09:10", ["X", "A", "B", "C", "T"], capacity: 1),
            TestSession("09:00", "09:10", ["X", "A", "B", "D", "T"], capacity: 1),
            TestSession("09:00", "09:10", ["X", "A", "C", "D", "T"], capacity: 1),
            TestSession("09:00", "09:10", ["X", "B", "C", "D", "T"], capacity: 1),
            //the permutations are ABC,ABD,ACD,BCD
            TestSession("09:00", "09:10", ["A"], capacity: 1),
            TestSession("09:00", "09:10", ["B"], capacity: 1),
            TestSession("09:00", "09:10", ["C"], capacity: 1),
            TestSession("09:00", "09:10", ["D"], capacity: 1),
            TestSession("09:00", "09:10", ["T"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        //only 9 of the 22 bookings can be honoured, but WHICH 9 matters!

        //X is assigned in the 'XBCDT' virtual slot.
        //T is assigned in its single slot
        //all 4 'A' bookings now have room using 'XABCT', 'XABDT', 'XACDT', and its single 'A' slot
        //and one of each B,C and D get a space in their single slots
        resultingAvailabilityState.Recalculations.Should().HaveCount(9);
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7", "11", "15");
    }

    [Fact]
    public async Task TheBestFitProblem()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Orange", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Blue"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3");
    }

    [Fact]
    public async Task TheBestFitProblem_2()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "C", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "B", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "B", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "A", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "A", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "D", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "A", avStatus: "Orphaned", creationOrder: 7),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B", "C"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "B", "D"], capacity: 2),
            TestSession("09:00", "09:10", ["A", "C", "D"], capacity: 2),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_3()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 4)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_4()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "A", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C", "D"], capacity: 4)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_5()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "B", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "E", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["A", "B", "C", "E"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "D"], capacity: 1),
            TestSession("09:00", "09:10", ["A", "F"], capacity: 1),
            TestSession("09:00", "09:10", ["B", "C"], capacity: 90)
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        // Bookings 1 and 2 can be booked
        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2");
    }

    [Fact]
    [InlineData(false)]
    [InlineData(true, true)]
    public async Task TheBestFitProblem_6()
    {
        //purple blue green
        var bookings = new List<Booking>
        {
            TestBooking("1", "Purple", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Purple", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Purple", "Green"], capacity: 1),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5");
    }

    /// <summary>
    ///     Prove adding extra orphaned bookings doesn't change the booked status of the first one
    /// </summary>
    [Fact]
    public async Task TheBestFitProblem_7()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Green", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Purple", avStatus: "Orphaned", creationOrder: 3),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple", "Orange"], capacity: 1)
        };

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState1 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState1.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");

        resultingAvailabilityState1.Recalculations.Should().NotContain(r => r.Booking.Reference == "2");
        resultingAvailabilityState1.Recalculations.Should().NotContain(r => r.Booking.Reference == "3");

        //now append orange orphaned bookings on, to make orange now the higher opportunity cost service
        bookings.AddRange([
            TestBooking("4", "Orange", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Orange", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Orange", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Orange", avStatus: "Orphaned", creationOrder: 7),
        ]);

        SetupAvailabilityAndBookings(bookings, sessions);


        var resultingAvailabilityState2 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState2.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1");

        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "2");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "3");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "4");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "5");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "6");
        resultingAvailabilityState2.Recalculations.Should().NotContain(r => r.Booking.Reference == "7");
    }

    /// <summary>
    ///     Prove that having the capacity across multiple sessions in any combination gives you the same outcome
    /// </summary>
    [Fact]
    public async Task TheBestFitProblem_8()
    {
        var bookings = new List<Booking>
        {
            TestBooking("1", "Blue", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Blue", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Blue", avStatus: "Orphaned", creationOrder: 5),
            TestBooking("6", "Green", avStatus: "Orphaned", creationOrder: 6),
            TestBooking("7", "Blue", avStatus: "Orphaned", creationOrder: 7), //should fail
            TestBooking("8", "Green", avStatus: "Orphaned", creationOrder: 8),
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 6), //single session with capcity 6
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState1 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));


        resultingAvailabilityState1.Recalculations.Where(r => r.Action == AvailabilityUpdateAction.SetToSupported)
            .Select(r => r.Booking.Reference).Should().BeEquivalentTo("1", "2", "3", "4", "5", "6", "7", "8");
        resultingAvailabilityState1.AvailableSlots.Should().BeEmpty();


        //third pass
        sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"],
                capacity: 2), //same capacity 6 divided by 3 sessions of length 2
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 2),
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState3 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState1.Recalculations.Should().BeEquivalentTo(resultingAvailabilityState3.Recalculations);
        resultingAvailabilityState1.AvailableSlots.Should().BeEquivalentTo(resultingAvailabilityState3.AvailableSlots);

        //second pass
        sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Blue"],
                capacity: 3), //same capacity 6 divided by 2 sessions of length 3
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 3),
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 2),
        };

        SetupAvailabilityAndBookings(bookings, sessions);

        var resultingAvailabilityState2 =
            await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1));

        resultingAvailabilityState1.Recalculations.Should().BeEquivalentTo(resultingAvailabilityState2.Recalculations);
        resultingAvailabilityState1.AvailableSlots.Should().BeEquivalentTo(resultingAvailabilityState2.AvailableSlots);
    }
}
