DECLARE
--PIDM.SPBPERS
----- GRAB ALL PIDMS NEEDED FOR LATER QUERIES ------------
Term varchar2 (50) := &Term;
--select SFRSTCR_PIDM from SFRSTCR where SFRSTCR_RSTS_CODE = (select STVRSTS_CODE from STVRSTS where STVRSTS_INCL_SECT_ENRL = 'Y') and SFRSTCR_TERM_CODE = ('') GROUP BY SFRSTCR_PIDM;
BEGIN 
----- QUERIES FOR INDIVIDUAL PIDMS -----------------------------------------------------
select coalesce('37',null) into :UTI;
select SPBPERS_SSN into :SSN from SPBPERS where SPBPERS_PIDM = ( :INPUT_INSERT_PIDM );
select SPRIDEN_FIRST_NAME into :FirstName from SPRIDEN where SPRIDEN_PIDM = ( :INPUT_INSERT_PIDM );
select SPRIDEN_MI into :MidName from SPRIDEN where SPRIDEN_PIDM = ( :INPUT_INSERT_PIDM );
select SPRIDEN_LAST_NAME into :LastName from SPRIDEN where SPRIDEN_PIDM = ( :INPUT_INSERT_PIDM );
select SPBPERS_NAME_SUFFIX into :nameSuffix from SPBPERS where SPBPERS_PIDM = ( :INPUT_INSERT_PIDM );
select SPBPERS_SEX into :Sex from SPBPERS where SPBPERS_PIDM = ( :INPUT_INSERT_PIDM );
select SPBPERS_BIRTH_DATE into :bDate from SPBPERS where SPBPERS_PIDM = ( :INPUT_INSERT_PIDM );
select SGBSTDN_RESD_CODE into :resident from SGBSTDN where SGBSTDN_TERM_CODE_EFF = (select SGBSTDN_TERM_CODE_EFF from SGBSTDN where SGBSTDN_PIDM = ( :INPUT_INSERT_PIDM ) and  SGBSTDN_TERM_CODE_EFF <= ( :INPUT_INSERT_TERMCODE ) order by SGBSTDN_TERM_CODE_EFF desc fetch first 1 ROW ONLY ) and SGBSTDN_PIDM = ( :INPUT_INSERT_PIDM ); 
select SPBPERS_CITZ_CODE into :citizen from SPBPERS where SPBPERS_PIDM = ( :INPUT_INSERT_PIDM );
select coalesce(a.SGBUSER_SUDD_CODE, b.SABSUPL_NATN_CODE_BIRTH) into :NationCode from SGBUSER a, SABSUPL b where a.SGBUSER_PIDM = ( :INPUT_INSERT_PIDM ) and ( b.SABSUPL_PIDM = ( :INPUT_INSERT_PIDM ) and ( b.SABSUPL_TERM_CODE_ENTRY <= :INPUT_INSERT_TERMCODE ) );
select SPRADDR_ZIP into :zip from SPRADDR where (SPRADDR_STATUS_IND is Null) and (coalesce( (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('PR')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('MA')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('DF')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))))) and (max(SPRADDR_SEQNO)); --not fin
select SPRADDR_STAT_CODE into :states from SPRADDR where SPRADDR_STATUS_IND is Null and coalesce( (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('PR')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('MA')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('DF')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO)))) and max(SPRADDR_SEQNO); --not fin
select substr(SPRADDR_CNTY_CODE,-1) into :county from SPRADDR where SPRADDR_STATUS_IND is Null and coalesce( (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('PR')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('MA')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO))), (select SPRADDR_ATYP_CODE from SPRADDR where (SPRADDR_ATYP_CODE = ('DF')) and (SPRADDR_STATUS_IND is Null) and (max(SPRADDR_SEQNO)))) and max(SPRADDR_SEQNO); --not fin

select SGBUSER_SUDA_CODE into :PRE_REG_TYPE from SGBUSER where SGBUSER_TERM_CODE = ( :INPUT_INSERT_TERMCODE ) and SGBUSER_PIDM = ( :INPUT_INSERT_PIDM );
--Reg type
select SHRLGPA_HOURS_EARNED into :CUMULATIVE_HOURS_EARNED from SHRLGPA as G, SGBSTDN as T where T.SGBSTDN_LEVL_CODE =  G.SHRLGPA_LEVL_CODE and G.SHRLGPA_GPA_TYPE_IND = ('O') and G.SHRLGPA_PIDM = ( :INPUT_INSERT_PIDM );

select SHRLGPA_GPA into :CUMULATIVE_HOME_GPA from SHRLGPA as G, SGBSTDN as T where T.SGBSTDN_LEVL_CODE =  G.SHRLGPA_LEVL_CODE and G.SHRLGPA_GPA_TYPE_IND = ('I') and G.SHRLGPA_PIDM = ( :INPUT_INSERT_PIDM );

select SHRLGPA_HOURS_ATTEMPED into :CUMULATIVE_HOURS_ATTEMPT from SHRLGPA as G, SGBSTDN as T where T.SGBSTDN_LEVL_CODE =  G.SHRLGPA_LEVL_CODE and G.SHRLGPA_GPA_TYPE_IND = ('O') and G.SHRLGPA_PIDM = ( :INPUT_INSERT_PIDM );

select SHRLGPA_GPA into :CUMULATIVE_OVERALL_GPA from SHRLGPA as G, SGBSTDN as T where T.SGBSTDN_LEVL_CODE =  G.SHRLGPA_LEVL_CODE and G.SHRLGPA_GPA_TYPE_IND = ('O') and G.SHRLGPA_PIDM = ( :INPUT_INSERT_PIDM );

select SORHSCH_SBGI_CODE into :HS_CODE from SORHSCH where max(SORHSCH_GRADUATION_DATE) and SORHSCH_PIDM = ( :INPUT_INSERT_PIDM ); 

select max(SORHSCH_GRADUATION_DATE) into :HS_GRAD_DATE FROM SORHSCH where SORHSCH_PIDM = ( :INPUT_INSERT_PIDM );

--need to make row to find number of nulls put in

--Previous Registration Type
--Registration
--Student Level, why get admin code, type, and major? why use stvlevl as reference
--

4,7,14,18,26,30
--student level = ug
--termcode input = direct code
--thec knows codes, no need for reference 
--write both proper documentation and easy documentation

----- End of File
 END;