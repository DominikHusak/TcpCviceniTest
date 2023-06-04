-- Vytvoření databáze
CREATE DATABASE pozemni_stavby;

-- Použití databáze
USE pozemni_stavby;

-- Vytvoření tabulky "stavba"
CREATE TABLE stavba (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nazev VARCHAR(255) NOT NULL,
  adresa VARCHAR(255) NOT NULL,
  UNIQUE (nazev)
);

-- Vytvoření tabulky "delnik"
CREATE TABLE delnik (
  id INT PRIMARY KEY AUTO_INCREMENT,
  jmeno VARCHAR(255) NOT NULL,
  prijmeni VARCHAR(255) NOT NULL,
  vek INT NOT NULL,
  stavba_id INT,
  pozice_id INT,
  FOREIGN KEY (stavba_id) REFERENCES stavba(id),
  FOREIGN KEY (pozice_id) REFERENCES pozice(id)
);

-- Vytvoření tabulky "pozice"
CREATE TABLE pozice (
  id INT PRIMARY KEY AUTO_INCREMENT,
  nazev VARCHAR(255) NOT NULL,
  UNIQUE (nazev)
);

-- Vložení testovacích dat do tabulky "stavba"
INSERT INTO stavba (nazev, adresa) VALUES
  ('Stavba A', 'Adresa A'),
  ('Stavba B', 'Adresa B'),
  ('Stavba C', 'Adresa C'),
  ('Stavba D', 'Adresa D'),
  ('Stavba E', 'Adresa E');

-- Vložení testovacích dat do tabulky "pozice"
INSERT INTO pozice (nazev) VALUES
  ('Zednik'),
  ('Tesar'),
  ('Malir'),
  ('Kamenik'),
  ('Instalater');

-- Vložení testovacích dat do tabulky "delnik"
INSERT INTO delnik (jmeno, prijmeni, vek, stavba_id, pozice_id) VALUES
  ('Jan', 'Novak', 30, 1, 1),
  ('Petr', 'Svoboda', 25, 1, 2),
  ('Martin', 'Kovac', 35, 2, 3),
  ('David', 'Cerny', 28, 2, 4),
  ('Michal', 'Prochazka', 32, 3, 5);

-- výpis stavby, dělníků a jejich pozic setříděných podle názvu stavby, názvu pozice a příjmení a jména dělníka abecedně
CREATE VIEW pohled_a AS
SELECT s.nazev AS nazev_stavby, d.jmeno, d.prijmeni, p.nazev AS nazev_pozice
FROM stavba s
JOIN delnik d ON s.id = d.stavba_id
JOIN pozice p ON d.pozice_id = p.id
ORDER BY nazev_stavby, nazev_pozice, d.prijmeni, d.jmeno;

-- výpis počtu dělníků na jednotlivých pozicích u jednotlivých staveb
CREATE VIEW pohled_b AS
SELECT s.nazev AS nazev_stavby, p.nazev AS nazev_pozice, COUNT(*) AS pocet_delniku
FROM stavba s
JOIN delnik d ON s.id = d.stavba_id
JOIN pozice p ON d.pozice_id = p.id
GROUP BY s.nazev, p.nazev;

-- výpis všech změn stavby u dělníků z tabulky "zmeny_delnici"
CREATE VIEW pohled_c AS
SELECT d.jmeno, d.prijmeni, s.nazev AS nazev_stavby, p.nazev AS nazev_pozice, z.typ_zmeny, z.cas
FROM zmeny_delnici z
JOIN delnik d ON z.delnik_id = d.id
JOIN stavba s ON z.stavba_id = s.id
JOIN pozice p ON z.pozice_id = p.id;

-- zápis údajů o odchodu dělníka ze stavby (delete) do záložní tabulky "zmeny_delnici"
CREATE TRIGGER odchod_trigger
AFTER DELETE ON delnik
FOR EACH ROW
BEGIN
  INSERT INTO zmeny_delnici (delnik_id, stavba_id, pozice_id, typ_zmeny, cas)
  VALUES (OLD.id, OLD.stavba_id, OLD.pozice_id, 'odchod', NOW());
END;

-- zápis údajů o příchodu dělníka na stavbu (insert) do záložní tabulky "zmeny_delnici"
CREATE TRIGGER prichod_trigger
AFTER INSERT ON delnik
FOR EACH ROW
BEGIN
  INSERT INTO zmeny_delnici (delnik_id, stavba_id, pozice_id, typ_zmeny, cas)
  VALUES (NEW.id, NEW.stavba_id, NEW.pozice_id, 'prichod', NOW());
END;

-- vložení dělníka na stavbu
DELIMITER //
CREATE PROCEDURE vlozeni_delnika_na_stavbu (
  IN p_stavba_nazev VARCHAR(255),
  IN p_prijmeni VARCHAR(255),
  IN p_jmeno VARCHAR(255),
  IN p_vek INT
)
BEGIN
  DECLARE p_stavba_id INT;
  
  SELECT id INTO p_stavba_id FROM stavba WHERE nazev = p_stavba_nazev;
  
  INSERT INTO delnik (jmeno, prijmeni, vek, stavba_id)
  VALUES (p_jmeno, p_prijmeni, p_vek, p_stavba_id);
END //
DELIMITER ;

-- změna pozice dělníka.
DELIMITER //
CREATE PROCEDURE zmena_pozice_delnika (
  IN p_stavba_nazev VARCHAR(255),
  IN p_prijmeni VARCHAR(255),
  IN p_jmeno VARCHAR(255),
  IN p_nova_pozice_nazev VARCHAR(255)
)
BEGIN
  DECLARE p_stavba_id INT;
  DECLARE p_nova_pozice_id INT;
  
  SELECT id INTO p_stavba_id FROM stavba WHERE nazev = p_stavba_nazev;
  SELECT id INTO p_nova_pozice_id FROM pozice WHERE nazev = p_nova_pozice_nazev;
  
  UPDATE delnik
  SET pozice_id = p_nova_pozice_id
  WHERE prijmeni = p_prijmeni AND jmeno = p_jmeno AND stavba_id = p_stavba_id;
END //
DELIMITER ;

-- změnu pozice dělníka
DELIMITER //
CREATE PROCEDURE zmena_pozice_delnika (
  IN p_stavba_nazev VARCHAR(255),
  IN p_prijmeni VARCHAR(255),
  IN p_jmeno VARCHAR(255),
  IN p_nova_pozice_nazev VARCHAR(255)
)
BEGIN
  DECLARE p_stavba_id INT;
  DECLARE p_nova_pozice_id INT;
  
  SELECT id INTO p_stavba_id FROM stavba WHERE nazev = p_stavba_nazev;
  SELECT id INTO p_nova_pozice_id FROM pozice WHERE nazev = p_nova_pozice_nazev;
  
  UPDATE delnik
  SET pozice_id = p_nova_pozice_id
  WHERE prijmeni = p_prijmeni AND jmeno = p_jmeno AND stavba_id = p_stavba_id;
END //
DELIMITER ;
