import express from "express";
import { Client } from "pg";
import bcrypt from "bcrypt";

const db = new Client({
  connectionString: Bun.env.POSTGRES_URI,
});

const app = express();
app.use("json");

app.post("/users/signup", async (req, res) => {
  const { username, password } = req.body;

  const hashedPassword = await bcrypt.hash(password, 10);
  db.query(
    `INSERT INTO users (username, password) VALUES ('${username}', '${hashedPassword}');`
  )
    .then(() => {
      res.status(201).json({
        user: {
          username,
          hashedPassword,
        },
      });
    })
    .catch((err) => {
      res.status(409).send("User already exists");
    });
});

app.post("/users/login", async (req, res) => {
  const { username, password } = req.body;

  const hashedPassword = await bcrypt.hash(password, 10);
  const result = await db.query(`
    SELECT *
    FROM users
    WHERE username = '${username}'
    AND password = '${hashedPassword}';
  `);
});

const PORT = Bun.env.APP_PORT;
db.connect().then(() => {
  app.listen(Bun.env.APP_PORT, () => {
    console.log(`Listening on port ${PORT}`);
  });
});
