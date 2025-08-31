-- Add Estado column to inmuebles table
-- This script adds the Estado column to track active/inactive state of properties

-- Add Estado column (boolean, default true for active)
ALTER TABLE inmuebles 
ADD COLUMN Estado BOOLEAN NOT NULL DEFAULT TRUE;


ALTER TABLE inmuebles 
	CHANGE Estado Estado BOOLEAN NOT NULL DEFAULT TRUE ;

-- Update existing records to be active by default
UPDATE inmuebles SET Estado = TRUE WHERE Estado IS NULL;

-- Add index for better performance when filtering by Estado
CREATE INDEX idx_inmuebles_estado ON inmuebles(Estado);

-- Verify the changes
SELECT 'Column Estado added successfully' as Result;
DESCRIBE inmuebles;
