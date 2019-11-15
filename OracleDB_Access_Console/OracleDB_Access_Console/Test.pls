--In Progress by Garrett
DECLARE
CLOSE_TERM VARCHAR2(24) := select SGBSTDN_TERM_CODE_EFF from SGBSTDN where SGBSTDN_PIDM = ( :INPUT_INSERT_PIDM ) and  SGBSTDN_TERM_CODE_EFF <= ( :INPUT_INSERT_TERMCODE ) order by SGBSTDN_TERM_CODE_EFF desc fetch first 1 ROW ONLY;
HOURS_EARNED NUMBER(22,11) := select SHRLGPA_HOURS_EARNED from SHRLGPA where (SHRLGPA_LEVL_CODE = 'UG') and (SHRLGPA_GPA_TYPE_IND = ('O')) and (SHRLGPA_PIDM = ( :INPUT_INSERT_PIDM ));

BEGIN

select case (select SGBSTDN_LEVL_CODE from SGBSTDN where SGBSTDN_TERM_CODE_EFF = ( CLOSE_TERM ) and SGBSTDN_PIDM = ( :INPUT_INSERT_PIDM ) )
    when 'GR' then 
    when 'CE' then 
    when 'U2' then 06
    when 'UG' then (case (select SGBSTDN_MAJR_CODE_1 from SGBSTDN where SGBSTDN_TERM_CODE_EFF = ( CLOSE_TERM ) and SGBSTDN_PIDM = ( :INPUT_INSERT_PIDM ) )
        when '2000' then 06
        when '0' then 06
        else (case
                when (HOURS_EARNED) <= 29.9 then 01
                when (HOURS_EARNED) between 29.9 and 59.9 then 02
                when (HOURS_EARNED) between 59.9 and 89.9 then 03
                else 04
            end)
        end)
    else 'not defined'
end from SGBSTDN;


select case (select SGBSTDN_LEVL_CODE from SGBSTDN where SGBSTDN_TERM_CODE_EFF = (201940) and SGBSTDN_PIDM = ('960203040') )
    when 'UG' then (case (select SGBSTDN_MAJR_CODE_1 from SGBSTDN where SGBSTDN_TERM_CODE_EFF = (201940) and SGBSTDN_PIDM = ('960203040') )
        when '2000' then 06
        when '0' then 06
        else (case )




--what is the difference between shrlgpa and shrtgpa?


--student level = ug
--termcode input = direct code
--thec knows codes, no need for reference 
--write both proper documentation and easy documentation