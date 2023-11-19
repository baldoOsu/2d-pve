import "dotenv/config";
import express from "express";
import { Client } from "pg";
import bcrypt from "bcrypt";

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
    );`;

  db.query(query, [username, hashedPassword])
    .then((queryRes) => {
      if (queryRes.rowCount === 1) {
        res.status(201).send();
        return;
      }

      res.status(401).send("User already exists");
    })
    .catch((err) => {
      console.log(err);
      res.status(500).send("Some unknown error happened");
    });
});

app.post("/users/login", async (req, res) => {
  const { username, password } = req.body;

  const query = `
    SELECT (password, highscore, games_played)
    FROM users
    WHERE username = $1::varchar;
  `;

  const result = await db.query(query, [username]);

  if (result.rows.length === 0) {
    res.status(401).send("Username not found");
    return;
  }

  let [dbPassword, dbHighscore, dbGamesPlayed] = result.rows[0].row
    .replace("(", "")
    .replace(")", "")
    .split(",");

  dbHighscore = parseInt(dbHighscore);
  dbGamesPlayed = parseInt(dbGamesPlayed);

  const isCorrectPw = await bcrypt.compare(password, dbPassword);
  if (isCorrectPw) {
    res.status(200).send({
      highscore: dbHighscore,
      gamesPlayed: dbGamesPlayed,
    });
    return;
  }

  res.status(401).send();
});

const PORT = process.env.APP_PORT;
db.connect().then(() => {
  app.listen(PORT, () => {
    console.log(`Listening on port ${PORT}`);
  });
});
