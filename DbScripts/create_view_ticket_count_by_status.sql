-- View: Ticket count by status
-- Purpose: Optional SQL View for reporting ticket distribution by status

CREATE OR ALTER VIEW vw_TicketCountByStatus AS
SELECT
    CASE Status
        WHEN 0 THEN 'Open'
        WHEN 1 THEN 'InProgress'
        WHEN 2 THEN 'Resolved'
        WHEN 3 THEN 'Closed'
        ELSE 'Unknown'
    END AS StatusName,
    COUNT(*) AS TicketCount
FROM Tickets
GROUP BY Status;
GO
