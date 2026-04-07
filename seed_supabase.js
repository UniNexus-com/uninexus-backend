const { Client } = require('pg');

const connectionString = "postgres://postgres.bpdsydscpveigfwrpbmo:MBhTdJmhqrcSgLMQ@aws-1-eu-central-1.pooler.supabase.com:6543/postgres?sslmode=require";

async function seed() {
    const client = new Client({
        connectionString,
        ssl: { rejectUnauthorized: false }
    });

    try {
        await client.connect();
        console.log('Connected to Supabase');

        // 1. Create Identity Roles if they don't exist
        await client.query(`
            INSERT INTO roles ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            SELECT '1', 'SKS_ADMIN', 'SKS_ADMIN', gen_random_uuid()::text WHERE NOT EXISTS (SELECT 1 FROM roles WHERE "Name" = 'SKS_ADMIN');
            INSERT INTO roles ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            SELECT '2', 'CLUB_LEADER', 'CLUB_LEADER', gen_random_uuid()::text WHERE NOT EXISTS (SELECT 1 FROM roles WHERE "Name" = 'CLUB_LEADER');
            INSERT INTO roles ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            SELECT '3', 'STUDENT', 'STUDENT', gen_random_uuid()::text WHERE NOT EXISTS (SELECT 1 FROM roles WHERE "Name" = 'STUDENT');
        `);

        // 2. Create Users (hashed password for "Leader123!" is AQAAAAIAAYagAAAAEJ...)
        const leaderId = 'f7457723-d8c8-47c1-8b04-91e68eeaed02';
        await client.query(`
            INSERT INTO users (id, "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "FullName")
            SELECT '${leaderId}', 'clubleader', 'CLUBLEADER', 'leader@akdeniz.edu.tr', 'LEADER@AKDENIZ.EDU.TR', true, 'AQAAAAIAAYagAAAAEJvR3n5z6m4Zc1QJj8W/M2oD4Z36cWbNnMhEHQc9X157H2w3zJzXGvZqjD4pLQ2bHw==', gen_random_uuid()::text, gen_random_uuid()::text, true, false, true, 0, 'Kulüp Lideri'
            WHERE NOT EXISTS (SELECT 1 FROM users WHERE id = '${leaderId}');

            INSERT INTO user_roles ("UserId", "RoleId")
            SELECT '${leaderId}', '2' WHERE NOT EXISTS (SELECT 1 FROM user_roles WHERE "UserId" = '${leaderId}' AND "RoleId" = '2');
        `);

        // 3. Create Clubs
        const clubRes = await client.query(`
            INSERT INTO clubs (name, description, is_active, created, created_by)
            VALUES ('Yazılım ve Teknoloji Kulübü', 'Yazılım geliştirme ve teknoloji projeleri.', true, NOW(), 'seed')
            ON CONFLICT DO NOTHING
            RETURNING id;
        `);
        
        let clubId;
        if (clubRes.rows.length > 0) {
            clubId = clubRes.rows[0].id;
        } else {
            const res = await client.query("SELECT id FROM clubs WHERE name = 'Yazılım ve Teknoloji Kulübü'");
            clubId = res.rows[0].id;
        }

        // 4. Create Club Roles and Privileges
        await client.query(`
            INSERT INTO club_roles (name, is_system_role, "Created", "CreatedBy")
            SELECT 'President', true, NOW(), 'seed'
            WHERE NOT EXISTS (SELECT 1 FROM club_roles WHERE name = 'President');

            INSERT INTO club_privileges (name, description)
            SELECT 'Manage Members', 'Can approve/reject join requests'
            WHERE NOT EXISTS (SELECT 1 FROM club_privileges WHERE name = 'Manage Members');
        `);

        const presidentRoleRes = await client.query("SELECT id FROM club_roles WHERE name = 'President'");
        const presidentRoleId = presidentRoleRes.rows[0].id;

        const privRes = await client.query("SELECT id FROM club_privileges WHERE name = 'Manage Members'");
        const privId = privRes.rows[0].id;

        await client.query(`
            INSERT INTO club_role_privileges (club_role_id, privilege_id)
            VALUES (${presidentRoleId}, ${privId})
            ON CONFLICT DO NOTHING;
        `);

        // 5. Link Leader to Club
        await client.query(`
            INSERT INTO user_clubs (user_id, club_id, club_role_id, join_date, is_active, "Id", "Created", "CreatedBy")
            VALUES ('${leaderId}', ${clubId}, ${presidentRoleId}, NOW(), true, 1, NOW(), 'seed')
            ON CONFLICT (user_id, club_id) DO UPDATE SET club_role_id = EXCLUDED.club_role_id;
        `);

        console.log('Seeding complete!');

    } catch (err) {
        console.error('Seeding ERROR:', err);
    } finally {
        await client.end();
    }
}

seed();
