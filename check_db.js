const { Client } = require('pg');

const connectionString = "Host=aws-1-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.bpdsydscpveigfwrpbmo;Password=MBhTdJmhqrcSgLMQ;SslMode=Require";

async function check() {
    const client = new Client({
        connectionString,
        ssl: { rejectUnauthorized: false }
    });

    try {
        await client.connect();
        const userCount = await client.query('SELECT count(*) FROM users');
        console.log('User count:', userCount.rows[0].count);
        
        const clubCount = await client.query('SELECT count(*) FROM clubs');
        console.log('Club count:', clubCount.rows[0].count);

        const roleCount = await client.query('SELECT count(*) FROM club_roles');
        console.log('Club Role count:', roleCount.rows[0].count);

        const privilegeCount = await client.query('SELECT count(*) FROM club_privileges');
        console.log('Club Privilege count:', privilegeCount.rows[0].count);

        const membershipCount = await client.query('SELECT count(*) FROM user_clubs');
        console.log('User Club Membership count:', membershipCount.rows[0].count);

        const requestCount = await client.query('SELECT count(*) FROM club_join_requests');
        console.log('Club Join Request count:', requestCount.rows[0].count);

        const users = await client.query('SELECT id, "Email", "FullName" FROM users LIMIT 10');
        console.log('Users:', JSON.stringify(users.rows, null, 2));

        const userClubs = await client.query('SELECT uc.user_id, u."Email", c.name as club_name, cr.name as role_name FROM user_clubs uc JOIN users u ON u.id = uc.user_id JOIN clubs c ON c.id = uc.club_id JOIN club_roles cr ON cr.id = uc.club_role_id');
        console.log('Active Memberships:', JSON.stringify(userClubs.rows, null, 2));

    } catch (err) {
        console.error('Error:', err);
    } finally {
        await client.end();
    }
}

check();
