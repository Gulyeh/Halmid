-- MySQL dump 10.13  Distrib 8.0.20, for Win64 (x86_64)
--
-- Host: localhost    Database: helmid_db
-- ------------------------------------------------------
-- Server version	8.0.21

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `bans`
--

DROP TABLE IF EXISTS `bans`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bans` (
  `idbans` int NOT NULL AUTO_INCREMENT,
  `channel_id` varchar(45) NOT NULL,
  `banned_id` varchar(45) NOT NULL,
  `admin_id` varchar(45) NOT NULL,
  `reason` varchar(45) DEFAULT NULL,
  `duration` varchar(45) NOT NULL,
  `ban_date` varchar(45) NOT NULL,
  `ban_expire` varchar(45) NOT NULL,
  PRIMARY KEY (`idbans`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bans`
--

LOCK TABLES `bans` WRITE;
/*!40000 ALTER TABLE `bans` DISABLE KEYS */;
/*!40000 ALTER TABLE `bans` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `blocked`
--

DROP TABLE IF EXISTS `blocked`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `blocked` (
  `idBlocked` int NOT NULL AUTO_INCREMENT,
  `Blocker_ID` varchar(45) NOT NULL,
  `Blocked_ID` varchar(45) NOT NULL,
  `Channel_ID` varchar(45) NOT NULL,
  PRIMARY KEY (`idBlocked`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `blocked`
--

LOCK TABLES `blocked` WRITE;
/*!40000 ALTER TABLE `blocked` DISABLE KEYS */;
/*!40000 ALTER TABLE `blocked` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `channels`
--

DROP TABLE IF EXISTS `channels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `channels` (
  `channel_key` varchar(50) NOT NULL,
  `channel_name` varchar(45) NOT NULL,
  `channel_admin` varchar(45) NOT NULL,
  `image` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`channel_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `channels`
--

LOCK TABLES `channels` WRITE;
/*!40000 ALTER TABLE `channels` DISABLE KEYS */;
/*!40000 ALTER TABLE `channels` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `friends`
--

DROP TABLE IF EXISTS `friends`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `friends` (
  `idfriends` int NOT NULL AUTO_INCREMENT,
  `user1_id` varchar(45) NOT NULL,
  `user2_id` varchar(45) NOT NULL,
  `status` varchar(45) NOT NULL,
  PRIMARY KEY (`idfriends`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `friends`
--

LOCK TABLES `friends` WRITE;
/*!40000 ALTER TABLE `friends` DISABLE KEYS */;
/*!40000 ALTER TABLE `friends` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `messages_history`
--

DROP TABLE IF EXISTS `messages_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `messages_history` (
  `idmessages` int NOT NULL AUTO_INCREMENT,
  `message` varchar(15000) DEFAULT NULL,
  `idsender` varchar(45) NOT NULL,
  `idchannel` varchar(45) NOT NULL,
  `time` varchar(45) NOT NULL,
  `edited` int DEFAULT NULL,
  `image` varchar(45) DEFAULT NULL,
  `decrypt_key` varchar(150) DEFAULT NULL,
  PRIMARY KEY (`idmessages`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `messages_history`
--

LOCK TABLES `messages_history` WRITE;
/*!40000 ALTER TABLE `messages_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `messages_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `private_channels`
--

DROP TABLE IF EXISTS `private_channels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `private_channels` (
  `channelID` varchar(45) NOT NULL,
  `user1_id` varchar(45) NOT NULL,
  `user2_id` varchar(45) NOT NULL,
  PRIMARY KEY (`channelID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `private_channels`
--

LOCK TABLES `private_channels` WRITE;
/*!40000 ALTER TABLE `private_channels` DISABLE KEYS */;
/*!40000 ALTER TABLE `private_channels` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `update_allow`
--

DROP TABLE IF EXISTS `update_allow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `update_allow` (
  `Update_Client` int NOT NULL,
  `Update_Version` varchar(45) NOT NULL,
  PRIMARY KEY (`Update_Client`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `update_allow`
--

LOCK TABLES `update_allow` WRITE;
/*!40000 ALTER TABLE `update_allow` DISABLE KEYS */;
INSERT INTO `update_allow` VALUES (0,'1.27');
/*!40000 ALTER TABLE `update_allow` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `loginid` varchar(50) NOT NULL,
  `login` varchar(100) NOT NULL,
  `name` varchar(45) NOT NULL,
  `status` varchar(45) NOT NULL,
  `avatar` varchar(55) DEFAULT NULL,
  PRIMARY KEY (`loginid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users_in_channels`
--

DROP TABLE IF EXISTS `users_in_channels`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users_in_channels` (
  `id` int NOT NULL AUTO_INCREMENT,
  `channel_id` varchar(45) NOT NULL,
  `user_id` varchar(45) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users_in_channels`
--

LOCK TABLES `users_in_channels` WRITE;
/*!40000 ALTER TABLE `users_in_channels` DISABLE KEYS */;
/*!40000 ALTER TABLE `users_in_channels` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-10-04 15:00:06
