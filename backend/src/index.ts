import "dotenv/config";
import express from "express";
import { Client } from "pg";
import bcrypt from "bcrypt";

import { signAccessToken, verifyAccessToken } from "./jwt";

const db = new Client({
  connectionString: process.env.POSTGRES_URI,
});

const app = express();
app.use(express.json());

app.post("/users/signup", async (req, res) => {
  const { username, password } = req.body;

  const hashedPassword = await bcrypt.hash(password, 10);
  const query = `
    INSERT INTO users (username, password)
    SELECT $1::varchar, $2::varchar
    WHERE NOT EXISTS (
      SELECT 1 FROM users WHERE username = $1::varchar
    )
    RETURNING *;`;

  db.query(query, [username, hashedPassword])
    .then(async (queryRes) => {
      if (queryRes.rowCount === 1) {
        const { id } = queryRes.rows[0];

        res.status(201).json({
          ID: id,
          AccessToken: await signAccessToken(id, username),
        });
        return;
      }

      res.status(409).send("User already exists");
    })
    .catch((err) => {
      console.log(err);
      res.status(500).send("Some unknown error happened");
    });
});

app.post("/users/login", async (req, res) => {
  const { username, password } = req.body;

  const query = `
    SELECT (id, password, highscore, games_played)
    FROM users
    WHERE username = $1::varchar;
  `;

  const result = await db.query(query, [username]).catch((err) => {
    console.error(err);
  });

  if (!result) {
    res.status(500).send("Something went wrong");
    return;
  }

  if (result.rows.length === 0) {
    res.status(401).send("Username not found");
    return;
  }

  let [dbId, dbPassword, dbHighscore, dbGamesPlayed] = result.rows[0].row
    .replace("(", "")
    .replace(")", "")
    .split(",");

  dbId = parseInt(dbId);
  dbHighscore = parseInt(dbHighscore);
  dbGamesPlayed = parseInt(dbGamesPlayed);

  const isCorrectPw = await bcrypt
    .compare(password, dbPassword)
    .catch((err) => {
      console.error(err);
    });

  if (isCorrectPw) {
    res.status(200).json({
      ID: dbId,
      HighScore: dbHighscore,
      GamesPlayed: dbGamesPlayed,
      AccessToken: await signAccessToken(dbId, username),
    });
    return;
  }

  res.status(401).send("Wrong password");
});

app.put("/users/:id/highscore", verifyAccessToken, (req, res) => {
  const id = parseInt(req.params.id);
  const highscore = req.body.highscore;

  if (highscore === undefined || typeof highscore != "number") {
    res.status(400).send("Invalid highscore");
    return;
  }

  const query = `
    UPDATE users
    SET highscore = $1::int
    WHERE ID = $2::int;
  `;

  db.query(query, [highscore, id])
    .then((queryRes) => {
      res.status(200).send();
    })
    .catch((err) => {
      console.error(err);
      res.status(500).send("Something went wrong");
    });
});

app.put("/users/:id/inc-games-played", verifyAccessToken, (req, res) => {
  const id = parseInt(req.params.id);
  const gamesPlayed = req.body.games_played;

  if (gamesPlayed === undefined || typeof gamesPlayed != "number") {
    res.status(400).send("Invalid games_played");
    return;
  }

  const query = `
    UPDATE users
    SET games_played = games_played + 1
    WHERE ID = $1::int
    AND games_played = $2
  `;

  db.query(query, [id, gamesPlayed])
    .then((queryRes) => {
      res.status(200).send();
    })
    .catch((err) => {
      console.error(err);
      res.status(500).send("Something went wrong");
    });
});

const PORT = process.env.APP_PORT;
db.connect().then(() => {
  app.listen(PORT, () => {
    console.log(`Listening on port ${PORT}`);
  });
});
