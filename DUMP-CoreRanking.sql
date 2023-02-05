-- MySQL dump 10.13  Distrib 8.0.28, for Win64 (x86_64)
--
-- Host: 192.168.18.185    Database: CoreRanking
-- ------------------------------------------------------
-- Server version	5.5.68-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Account`
--

DROP TABLE IF EXISTS `Account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Account` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Login` longtext,
  `Ip` longtext,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Account`
--

LOCK TABLES `Account` WRITE;
/*!40000 ALTER TABLE `Account` DISABLE KEYS */;
/*!40000 ALTER TABLE `Account` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Banned`
--

DROP TABLE IF EXISTS `Banned`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Banned` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RoleId` int(11) NOT NULL,
  `BanTime` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Banned_RoleId` (`RoleId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Banned`
--

LOCK TABLES `Banned` WRITE;
/*!40000 ALTER TABLE `Banned` DISABLE KEYS */;
/*!40000 ALTER TABLE `Banned` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Battle`
--

DROP TABLE IF EXISTS `Battle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Battle` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `KillerId` int(11) NOT NULL,
  `KilledId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Battle_KilledId` (`KilledId`) USING BTREE,
  KEY `IX_Battle_KillerId` (`KillerId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Battle`
--

LOCK TABLES `Battle` WRITE;
/*!40000 ALTER TABLE `Battle` DISABLE KEYS */;
/*!40000 ALTER TABLE `Battle` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Collect`
--

DROP TABLE IF EXISTS `Collect`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Collect` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ItemId` int(11) NOT NULL,
  `RoleId` int(11) NOT NULL,
  `Date` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Collect_RoleId` (`RoleId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Collect`
--

LOCK TABLES `Collect` WRITE;
/*!40000 ALTER TABLE `Collect` DISABLE KEYS */;
/*!40000 ALTER TABLE `Collect` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Hunt`
--

DROP TABLE IF EXISTS `Hunt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Hunt` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ItemId` int(11) NOT NULL,
  `RoleId` int(11) NOT NULL,
  `Date` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Hunt_RoleId` (`RoleId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Hunt`
--

LOCK TABLES `Hunt` WRITE;
/*!40000 ALTER TABLE `Hunt` DISABLE KEYS */;
/*!40000 ALTER TABLE `Hunt` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Prize`
--

DROP TABLE IF EXISTS `Prize`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Prize` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PrizeDeliveryDate` datetime NOT NULL,
  `PrizeOption` smallint(6) NOT NULL,
  `PrizeRewardType` smallint(6) NOT NULL,
  `WinCriteria` smallint(6) NOT NULL,
  `DeliveryCount` int(11) NOT NULL,
  `DeliveryRoleIdListAsJson` varchar(5000) NOT NULL,
  `CashCount` int(11) DEFAULT NULL,
  `ItemRewardId` int(11) DEFAULT NULL,
  `ItemRewardCount` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Prize`
--

LOCK TABLES `Prize` WRITE;
/*!40000 ALTER TABLE `Prize` DISABLE KEYS */;
/*!40000 ALTER TABLE `Prize` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `Role`
--

DROP TABLE IF EXISTS `Role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Role` (
  `RoleId` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `CharacterName` longtext,
  `CharacterClass` longtext,
  `CharacterGender` longtext,
  `Elo` longtext,
  `Level` int(11) NOT NULL,
  `RebornCount` tinyint(4) NOT NULL DEFAULT '0',
  `LevelDate` datetime NOT NULL DEFAULT '1900-01-01 00:00:00',
  `Points` int(11) NOT NULL,
  `Doublekill` int(11) NOT NULL,
  `Triplekill` int(11) NOT NULL,
  `Quadrakill` int(11) NOT NULL,
  `Pentakill` int(11) NOT NULL,
  `CollectPoint` double NOT NULL,
  `Death` int(11) NOT NULL DEFAULT '0',
  `Kill` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`RoleId`),
  KEY `IX_Role_AccountId` (`AccountId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `Role`
--

LOCK TABLES `Role` WRITE;
/*!40000 ALTER TABLE `Role` DISABLE KEYS */;
/*!40000 ALTER TABLE `Role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'CoreRanking'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-03-21  1:19:52
