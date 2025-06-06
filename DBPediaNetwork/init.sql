-- Criação das tabelas
CREATE TABLE IF NOT EXISTS User (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  email VARCHAR(100) NOT NULL UNIQUE,
  password VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS Node (
  id INT AUTO_INCREMENT PRIMARY KEY,
  label VARCHAR(255),
  uri VARCHAR(500) NOT NULL UNIQUE,
  isResource BOOLEAN NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS NodeRelation (
  id INT AUTO_INCREMENT PRIMARY KEY,
  node_dad_id INT NOT NULL,
  node_child_id INT NOT NULL,
  FOREIGN KEY (node_dad_id) REFERENCES Node(id),
  FOREIGN KEY (node_child_id) REFERENCES Node(id)
);

CREATE TABLE IF NOT EXISTS PopularNode (
  id INT AUTO_INCREMENT PRIMARY KEY,
  node_id INT NOT NULL,
  user_id INT NULL,
  search_date DATETIME DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (node_id) REFERENCES Node(id),
  FOREIGN KEY (user_id) REFERENCES User(id)
);

-- Criação das stored procedures
DELIMITER //

CREATE PROCEDURE P_SEL_AUTOCOMPLETE_SOURCE()
BEGIN
  SELECT DISTINCT uri FROM Node WHERE isResource = 1 ORDER BY uri LIMIT 1000;
END //

CREATE PROCEDURE P_INS_USER(IN p_name VARCHAR(100), IN p_password VARCHAR(100), IN p_email VARCHAR(100))
BEGIN
  INSERT INTO User(name, password, email) VALUES(p_name, p_password, p_email);
  SELECT * FROM User WHERE id = LAST_INSERT_ID();
END //

CREATE PROCEDURE P_INS_NODE(IN p_label VARCHAR(255), IN p_uri VARCHAR(500), IN p_isResource BOOLEAN)
BEGIN
  INSERT INTO Node(label, uri, isResource) VALUES(p_label, p_uri, p_isResource)
  ON DUPLICATE KEY UPDATE id=LAST_INSERT_ID(id), label = p_label;
  SELECT LAST_INSERT_ID();
END //

CREATE PROCEDURE P_SEL_NODE_ID_BY_URI(IN p_uri VARCHAR(500))
BEGIN
  SELECT id FROM Node WHERE uri = p_uri LIMIT 1;
END //

CREATE PROCEDURE P_INS_NODE_RELATION(IN p_node_dad_id INT, IN p_node_child_id INT)
BEGIN
  INSERT INTO NodeRelation(node_dad_id, node_child_id) VALUES(p_node_dad_id, p_node_child_id);
  SELECT ROW_COUNT();
END //

CREATE PROCEDURE P_SEL_LABEL_NODE(IN p_uri VARCHAR(500))
BEGIN
  SELECT label FROM Node WHERE uri = p_uri LIMIT 1;
END //

CREATE PROCEDURE P_INS_POPULAR_NODE(IN p_node_id INT, IN p_user_id INT)
BEGIN
  INSERT INTO PopularNode(node_id, user_id) VALUES(p_node_id, p_user_id);
  SELECT ROW_COUNT();
END //

CREATE PROCEDURE P_GET_POPULARS_NODES_BY_KEY(IN p_uri VARCHAR(500))
BEGIN
  SELECT n2.label, n2.uri, n2.isResource
  FROM Node n1
  JOIN NodeRelation nr ON n1.id = nr.node_dad_id
  JOIN Node n2 ON nr.node_child_id = n2.id
  WHERE n1.uri = p_uri
  ORDER BY n2.isResource DESC;
END //

DELIMITER ;