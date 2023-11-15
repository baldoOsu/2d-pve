import { NextFunction, Request, Response } from "express";
import JWT from "jsonwebtoken";

const secret = Bun.env.ACCESS_TOKEN_SECRET!;
export function signAccessToken(username: string) {
  return new Promise((resolve, reject) => {
    const payload = {};
    const options = {
      expiresIn: "1d",
      audience: username,
    };

    JWT.sign(payload, secret, options, (err, token) => {
      if (err) {
        reject(err.message);
        return;
      }

      resolve(token);
    });
  });
}

export function verifyAccessToken(
  req: Request,
  res: Response,
  next: NextFunction
) {
  const authHeader = req.headers["authorization"];
  if (!authHeader)
    return next({ code: 401, message: "No authorization header provided" });

  const token = authHeader.split(" ")[1];
  JWT.verify(token, secret, (err, payload) => {
    if (err) {
      if (err.name === "JsonWebTokenError")
        return next({ code: 401, message: "Invalid access token" });

      return next({
        code: 500,
        message: "Something went wrong at verifying access token",
      });
    }

    // så jeg kan modificere request
    // @ts-ignore
    req.payload = payload;
    next();
  });
}
