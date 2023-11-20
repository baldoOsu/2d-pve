"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.verifyAccessToken = exports.signAccessToken = void 0;
require("dotenv/config");
const jsonwebtoken_1 = __importDefault(require("jsonwebtoken"));
const secret = process.env.ACCESS_TOKEN_SECRET;
function signAccessToken(id, username) {
    return new Promise((resolve, reject) => {
        const payload = {
            id,
        };
        const options = {
            expiresIn: "90d",
            audience: username,
        };
        jsonwebtoken_1.default.sign(payload, secret, options, (err, token) => {
            if (err) {
                reject(err.message);
                return;
            }
            resolve(token);
        });
    });
}
exports.signAccessToken = signAccessToken;
function verifyAccessToken(req, res, next) {
    const authHeader = req.headers["authorization"];
    if (!authHeader)
        return next({ code: 401, message: "No authorization header provided" });
    const token = authHeader.split(" ")[1];
    jsonwebtoken_1.default.verify(token, secret, (err, payload) => {
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
exports.verifyAccessToken = verifyAccessToken;
