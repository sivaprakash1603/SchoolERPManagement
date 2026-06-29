const crypto = require('crypto');

const password = 'Admin@123';
const salt = crypto.randomBytes(16);
const iterations = 100000;
const keylen = 32;
const digest = 'sha256';

crypto.pbkdf2(password, salt, iterations, keylen, digest, (err, derivedKey) => {
    if (err) throw err;
    const hashStr = `${salt.toString('base64')}:${derivedKey.toString('base64')}`;
    console.log(`UPDATE users SET passwordhash = '${hashStr}' WHERE username = 'admin';`);
});
