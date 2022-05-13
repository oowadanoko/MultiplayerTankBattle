drop database if exists `game`;
create database `game`;
use `game`;

drop table if exists `account`;
create table `account`
(
	`id` varchar(20) primary key not null,
	`pw` text
);

drop table if exists `player`;
create table `player`
(
	`id` varchar(20) primary key not null,
	`data` text
);