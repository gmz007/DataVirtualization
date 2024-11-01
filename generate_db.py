import sqlite3
import random
import time

from datetime import datetime, timedelta
from faker import Faker


def create_test_database(db_path='WpfApplication.Data/App.db', num_records=1_000_000):
    """
    Creates a test database with 1 million rows of realistic-looking data.
    Uses batch inserts and transactions for optimal performance.
    """
    # Initialize Faker for generating realistic data
    fake = Faker()

    # Connect to SQLite database
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Create table
    cursor.execute('''
    CREATE TABLE IF NOT EXISTS User (
        Id INTEGER PRIMARY KEY,
        Name TEXT,
        Email TEXT,
        RegistrationDate TIMESTAMP,
        LastLogin TIMESTAMP,
        Status TEXT,
        Points INTEGER,
        SubscriptionType TEXT
    )''')

    cursor.execute('''
    CREATE TABLE IF NOT EXISTS UserStats (
        TotalUsers INTEGER NOT NULL DEFAULT 0
    )''')

    # Prepare data generation parameters
    start_date = datetime(2020, 1, 1)
    status_options = ['active', 'inactive', 'suspended', 'pending']
    subscription_types = ['free', 'basic', 'premium', 'enterprise']

    # Batch size for insertions
    batch_size = 10_000
    total_batches = num_records // batch_size

    print(f"Generating {num_records:,} records...")
    start_time = time.time()

    try:
        # Begin transaction
        conn.execute('BEGIN TRANSACTION')

        for batch in range(total_batches):
            batch_data = []

            for _ in range(batch_size):
                reg_date = start_date + timedelta(
                    days=random.randint(0, 1460)  # Within last 4 years
                )
                last_login = reg_date + timedelta(
                    days=random.randint(0, (datetime.now() - reg_date).days)
                )

                record = (
                    fake.name(),
                    fake.email(),
                    reg_date.isoformat(),
                    last_login.isoformat(),
                    random.choice(status_options),
                    random.randint(0, 10000),
                    random.choice(subscription_types)
                )
                batch_data.append(record)

            # Insert batch
            cursor.executemany('''
                INSERT INTO User (Name, Email, RegistrationDate, LastLogin, 
                                 Status, Points, SubscriptionType)
                VALUES (?, ?, ?, ?, ?, ?, ?)
            ''', batch_data)

            # Progress update
            if (batch + 1) % 10 == 0:
                print(f"Progress: {((batch + 1) / total_batches) * 100:.1f}%")

        cursor.execute('''
            INSERT INTO UserStats (TotalUsers)
            SELECT COUNT(*) FROM User
        ''')

        # Commit transaction
        conn.commit()

        # Create indexes for better query performance
        print("Creating indexes...")
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_Name ON user(Name)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_RegistrationDate ON user(RegistrationDate)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_Status ON user(Status)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_SubscriptionType ON user(SubscriptionType)')
    
        cursor.execute('''
        CREATE TRIGGER IF NOT EXISTS AfterUserInsert
        AFTER INSERT ON User
        BEGIN
            UPDATE UserStats 
            SET TotalUsers = TotalUsers + 1;
        END''')
        
        cursor.execute('''
        CREATE TRIGGER IF NOT EXISTS AfterUserDelete
        AFTER DELETE ON User
        BEGIN
            UPDATE UserStats 
            SET TotalUsers = TotalUsers - 1;
        END;''')

    except Exception as e:
        conn.rollback()
        raise e
    finally:
        conn.close()

    end_time = time.time()
    print(f"\nCompleted in {end_time - start_time:.2f} seconds")


if __name__ == '__main__':
    create_test_database()