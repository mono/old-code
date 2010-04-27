CREATE TABLE mp_collections (
	c_id		INTEGER PRIMARY KEY,
	c_name		VARCHAR(256)		NOT NULL,
	c_web_viewable	BOOLEAN			NOT NULL DEFAULT 'f'
);

CREATE TABLE mp_images (
	i_id		INTEGER PRIMARY KEY	NOT NULL,
	i_filename	VARCHAR(256)		NOT NULL,
	i_dirname	VARCHAR(256)		NOT NULL,
	i_width		INTEGER,
	i_height	INTEGER,
	i_caption	TEXT,
	i_public	BOOLEAN		NOT NULL DEFAULT 'f',

	i_filesize	INTEGER
);

CREATE TABLE mp_collection_images (
	c_id		INTEGER			NOT NULL,
	i_id		INTEGER			NOT NULL
);

CREATE TABLE mp_thumbnails (
	i_id		INTEGER			NOT NULL,
	thumbnail_file	VARCHAR(256)		NOT NULL
);

CREATE TABLE mp_keywords (
	k_id		INTEGER PRIMARY KEY	NOT NULL,
	k_name		VARCHAR(256)		NOT NULL
);

CREATE TABLE mp_image_keywords (
	i_id		INTEGER			NOT NULL,
	k_id		INTEGER			NOT NULL
);

CREATE TABLE mp_version (
	mp_db_version	VARCHAR(256)
);
INSERT INTO mp_version VALUES ('1');
