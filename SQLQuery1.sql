CREATE VIEW vw_BookingDetails AS
SELECT
    b.BookingID,
    b.BookingDate,
    e.Name AS EventName,
    v.Location AS VenueLocation,
    v.Capacity
FROM Bookings b
INNER JOIN Events e ON b.EventID = e.EventID
INNER JOIN Venues v ON b.VenueID = v.VenueID;