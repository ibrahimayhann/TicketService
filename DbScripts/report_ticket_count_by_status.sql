-- Report: Ticket count by status (readable)
-- Converts enum values to human-readable status names

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
GROUP BY Status
ORDER BY Status;
